using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using EcoDeLasCenizas.Core;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Gameplay
{
    public enum CharacterClass
    {
        Engineer,
        Explorer,
        Scientist,
        Tactician
    }

    public enum PolicyOption
    {
        OptionA_ProductionPriority,   // 70% Energy to Greenhouses
        OptionB_DefensePriority,      // 70% Energy to Wall Thermal Shields
        OptionC_OverchargeReactor     // Convert Ignicita into massive heat pulse
    }

    [System.Serializable]
    public class CouncilVoteSession
    {
        public string proposalTitle;
        public string proposalDescription;
        public float votingTimeRemaining = 30f;
        public bool isSessionActive = false;

        public Dictionary<PolicyOption, float> voteTally = new Dictionary<PolicyOption, float>();
        public HashSet<string> votedPlayerIDs = new HashSet<string>();
    }

    /// <summary>
    /// Manages democratic Council voting sessions over Mirror networking.
    /// Server reads player class directly from server-side PlayerController SyncVar to prevent spoofing.
    /// Rejects duplicate votes from the same netId.
    /// </summary>
    public class CouncilVotingManager : NetworkBehaviour
    {
        public static CouncilVotingManager Instance { get; private set; }

        [Header("Active Session")]
        [SerializeField] private CouncilVoteSession currentSession;

        [Header("Events")]
        public UnityEvent<CouncilVoteSession> OnVotingStarted;
        public UnityEvent<PolicyOption, float> OnVoteCast;
        public UnityEvent<PolicyOption> OnVotingEnded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Starts a new Council Voting Session on the Server.
        /// </summary>
        [Server]
        public void StartVotingSession(string title, string description, float duration = 30f)
        {
            currentSession = new CouncilVoteSession
            {
                proposalTitle = title,
                proposalDescription = description,
                votingTimeRemaining = duration,
                isSessionActive = true
            };

            currentSession.voteTally[PolicyOption.OptionA_ProductionPriority] = 0f;
            currentSession.voteTally[PolicyOption.OptionB_DefensePriority] = 0f;
            currentSession.voteTally[PolicyOption.OptionC_OverchargeReactor] = 0f;

            Debug.Log($"[CouncilVotingManager SERVER] VOTING STARTED: {title}");
            RpcNotifyVotingStarted(title, description, duration);
        }

        private void Update()
        {
            if (!isServer) return;

            if (currentSession != null && currentSession.isSessionActive)
            {
                currentSession.votingTimeRemaining -= Time.deltaTime;
                if (currentSession.votingTimeRemaining <= 0f)
                {
                    EndVotingSession();
                }
            }
        }

        /// <summary>
        /// Casts a vote signed with the sender's network connection identity.
        /// Reads true character class from server-side PlayerController SyncVar.
        /// </summary>
        [Command(requiresAuthority = false)]
        public void CmdCastVote(PolicyOption chosenOption, NetworkConnectionToClient senderConn = null)
        {
            if (currentSession == null || !currentSession.isSessionActive)
            {
                Debug.LogWarning("[CouncilVotingManager SERVER] Attempted to vote while no session is active.");
                return;
            }

            NetworkConnectionToClient conn = senderConn ?? connectionToClient;
            if (conn == null || conn.identity == null) return;

            string senderNetId = conn.connectionId.ToString();

            if (currentSession.votedPlayerIDs.Contains(senderNetId))
            {
                Debug.LogWarning($"[CouncilVotingManager SERVER] DUPLICATE VOTE REJECTED: NetId {senderNetId} has already voted!");
                return;
            }

            // Read true class from server-side PlayerController component
            CharacterClass serverPlayerClass = CharacterClass.Explorer;
            PlayerController pController = conn.identity.GetComponent<PlayerController>();
            if (pController != null)
            {
                serverPlayerClass = pController.Class;
            }

            if (!currentSession.voteTally.ContainsKey(chosenOption))
            {
                Debug.LogWarning($"[CouncilVotingManager SERVER] INVALID VOTE OPTION REJECTED: {chosenOption}");
                return;
            }

            currentSession.votedPlayerIDs.Add(senderNetId);

            float voteWeight = GetVoteWeightForClass(serverPlayerClass, chosenOption);
            currentSession.voteTally[chosenOption] += voteWeight;

            Debug.Log($"[CouncilVotingManager SERVER] Vote cast by NetId {senderNetId} (Class:{serverPlayerClass}) for {chosenOption} (Weight: {voteWeight}). New Total: {currentSession.voteTally[chosenOption]}");

            RpcNotifyVoteCast(chosenOption, currentSession.voteTally[chosenOption]);
        }

        /// <summary>
        /// Calculates vote multiplier weight based on class expertise.
        /// </summary>
        private float GetVoteWeightForClass(CharacterClass pClass, PolicyOption option)
        {
            switch (option)
            {
                case PolicyOption.OptionA_ProductionPriority:
                    return pClass == CharacterClass.Scientist ? 2.0f : 1.0f;

                case PolicyOption.OptionB_DefensePriority:
                    return (pClass == CharacterClass.Tactician || pClass == CharacterClass.Engineer) ? 2.0f : 1.0f;

                case PolicyOption.OptionC_OverchargeReactor:
                    return pClass == CharacterClass.Engineer ? 2.5f : 1.0f;

                default:
                    return 1.0f;
            }
        }

        /// <summary>
        /// Finalizes voting and resolves winning policy on Server.
        /// </summary>
        [Server]
        public void EndVotingSession()
        {
            if (currentSession == null || !currentSession.isSessionActive) return;

            currentSession.isSessionActive = false;

            PolicyOption winningOption = PolicyOption.OptionA_ProductionPriority;
            float highestVotes = -1f;

            foreach (var kvp in currentSession.voteTally)
            {
                if (kvp.Value > highestVotes)
                {
                    highestVotes = kvp.Value;
                    winningOption = kvp.Key;
                }
            }

            Debug.Log($"[CouncilVotingManager SERVER] VOTING CONCLUDED. Winning Policy: {winningOption} with {highestVotes} weighted votes.");
            ApplyPolicyEffects(winningOption);

            RpcNotifyVotingEnded(winningOption);
        }

        [Server]
        private void ApplyPolicyEffects(PolicyOption option)
        {
            switch (option)
            {
                case PolicyOption.OptionA_ProductionPriority:
                    Debug.Log("[Council Policy SERVER] +50% Greenhouse Efficiency active. Wall defenses unpowered.");
                    break;

                case PolicyOption.OptionB_DefensePriority:
                    Debug.Log("[Council Policy SERVER] Wall Thermal Shields Activated (+30% Wall Resistance).");
                    break;

                case PolicyOption.OptionC_OverchargeReactor:
                    Debug.Log("[Council Policy SERVER] Reactor Overcharged! +15°C Temperature surge.");
                    ReactorManager reactor = GameManager.Instance != null ? GameManager.Instance.GetReactorForCity(1) : FindObjectOfType<ReactorManager>();
                    if (reactor != null)
                    {
                        reactor.DepositIgnicita(50f, 1);
                    }
                    break;
            }
        }

        [ClientRpc]
        private void RpcNotifyVotingStarted(string title, string description, float duration)
        {
            Debug.Log($"[CouncilVotingManager RPC] Voting Started: {title}");
            OnVotingStarted?.Invoke(new CouncilVoteSession { proposalTitle = title, proposalDescription = description, votingTimeRemaining = duration, isSessionActive = true });
        }

        [ClientRpc]
        private void RpcNotifyVoteCast(PolicyOption option, float totalVotes)
        {
            OnVoteCast?.Invoke(option, totalVotes);
        }

        [ClientRpc]
        private void RpcNotifyVotingEnded(PolicyOption winningOption)
        {
            Debug.Log($"[CouncilVotingManager RPC] Voting Ended. Winner: {winningOption}");
            OnVotingEnded?.Invoke(winningOption);
        }
    }
}
