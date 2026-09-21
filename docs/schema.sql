-- =============================================================================
-- ESQUEMA DE BASE DE DATOS POSTGRESQL / SUPABASE
-- WHITEOUT SURVIVAL - ARQUITECTURA BACKEND DE GÉNESIS A REVELACIÓN
-- =============================================================================

-- Habilitar extensión para generación de UUIDs
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- -----------------------------------------------------------------------------
-- 1. FACCIONES Y ALIANZAS (CLANS / ALLIANCES)
-- -----------------------------------------------------------------------------
CREATE TABLE alliances (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(32) NOT NULL UNIQUE,
    tag VARCHAR(4) NOT NULL UNIQUE,
    leader_id UUID,
    power BIGINT DEFAULT 0,
    hq_x INT,
    hq_y INT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- -----------------------------------------------------------------------------
-- 2. JUGADORES Y ESTADO DE PERFIL
-- -----------------------------------------------------------------------------
CREATE TABLE players (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(32) NOT NULL UNIQUE,
    furnace_level INT DEFAULT 1 CHECK (furnace_level >= 1),
    fire_crystals INT DEFAULT 0 CHECK (fire_crystals >= 0),
    vip_level INT DEFAULT 0 CHECK (vip_level >= 0),
    power BIGINT DEFAULT 0,
    alliance_id UUID REFERENCES alliances(id) ON DELETE SET NULL,
    city_x INT DEFAULT 0,
    city_y INT DEFAULT 0,
    shield_until TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Vincular clave foránea de líder de alianza
ALTER TABLE alliances
    ADD CONSTRAINT fk_alliance_leader
    FOREIGN KEY (leader_id) REFERENCES players(id) ON DELETE SET NULL;

-- -----------------------------------------------------------------------------
-- 3. EDIFICIOS Y TIMERS DE CONSTRUCCIÓN
-- -----------------------------------------------------------------------------
CREATE TABLE player_buildings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id UUID NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    building_type VARCHAR(32) NOT NULL, -- 'furnace', 'sawmill', 'coal_mine', 'iron_mine', 'infirmary', 'kitchen', 'barracks'
    level INT DEFAULT 1 CHECK (level >= 1),
    upgrade_starts_at TIMESTAMP WITH TIME ZONE,
    upgrade_ends_at TIMESTAMP WITH TIME ZONE, -- NULL si no está en construcción
    CONSTRAINT unique_player_building UNIQUE (player_id, building_type)
);

-- -----------------------------------------------------------------------------
-- 4. ALMACÉN Y RECURSOS DEL JUGADOR
-- -----------------------------------------------------------------------------
CREATE TABLE player_resources (
    player_id UUID PRIMARY KEY REFERENCES players(id) ON DELETE CASCADE,
    wood BIGINT DEFAULT 10000 CHECK (wood >= 0),
    food BIGINT DEFAULT 10000 CHECK (food >= 0),
    coal BIGINT DEFAULT 5000 CHECK (coal >= 0),
    iron BIGINT DEFAULT 0 CHECK (iron >= 0),
    gems BIGINT DEFAULT 0 CHECK (gems >= 0),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- -----------------------------------------------------------------------------
-- 5. TROPAS, HOSPITALES Y OFICINA DE ALISTAMIENTO
-- -----------------------------------------------------------------------------
CREATE TABLE player_troops (
    player_id UUID NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    troop_type VARCHAR(16) NOT NULL CHECK (troop_type IN ('infantry', 'lancer', 'marksman')),
    tier INT NOT NULL CHECK (tier >= 1 AND tier <= 10),
    amount INT DEFAULT 0 CHECK (amount >= 0),
    wounded_amount INT DEFAULT 0 CHECK (wounded_amount >= 0),
    enlistment_amount INT DEFAULT 0 CHECK (enlistment_amount >= 0),
    PRIMARY KEY (player_id, troop_type, tier)
);

-- -----------------------------------------------------------------------------
-- 6. HÉROES Y COLECCIÓN (GACHA & GENERACIONES)
-- -----------------------------------------------------------------------------
CREATE TABLE heroes (
    code VARCHAR(32) PRIMARY KEY, -- 'solis', 'sergey', 'molly', 'bahiti'
    name VARCHAR(64) NOT NULL,
    generation INT DEFAULT 1 CHECK (generation >= 1),
    troop_type_affinity VARCHAR(16) CHECK (troop_type_affinity IN ('infantry', 'lancer', 'marksman', 'all')),
    rarity VARCHAR(16) NOT NULL CHECK (rarity IN ('SR', 'SSR', 'UR'))
);

CREATE TABLE player_heroes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id UUID NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    hero_code VARCHAR(32) NOT NULL REFERENCES heroes(code) ON DELETE CASCADE,
    level INT DEFAULT 1 CHECK (level >= 1 AND level <= 80),
    stars INT DEFAULT 1 CHECK (stars >= 1 AND stars <= 5),
    skill_level INT DEFAULT 1 CHECK (skill_level >= 1 AND skill_level <= 5),
    shards_count INT DEFAULT 0 CHECK (shards_count >= 0),
    CONSTRAINT unique_player_hero UNIQUE (player_id, hero_code)
);

-- -----------------------------------------------------------------------------
-- 7. MARCHAS DEL MAPA MUNDIAL (WORLD MAP MARCHES)
-- -----------------------------------------------------------------------------
CREATE TABLE marches (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id UUID NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    alliance_id UUID REFERENCES alliances(id) ON DELETE SET NULL,
    origin_x INT NOT NULL,
    origin_y INT NOT NULL,
    target_x INT NOT NULL,
    target_y INT NOT NULL,
    march_type VARCHAR(32) NOT NULL, -- 'ATTACK', 'GATHER', 'RALLY', 'REINFORCE', 'RETURN'
    start_time TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    arrival_time TIMESTAMP WITH TIME ZONE NOT NULL,
    state VARCHAR(16) NOT NULL CHECK (state IN ('MARCHING', 'GATHERING', 'RETURNING', 'COMBAT', 'COMPLETED')),
    troop_data JSONB NOT NULL, -- Array JSON con composición de tropas y héroes
    cargo_data JSONB DEFAULT '{}'::jsonb -- Recursos recolectados / saqueados
);

-- -----------------------------------------------------------------------------
-- 8. ESTRUCTURAS TERRITORIALES DE ALIANZA (HQ & BANNERS)
-- -----------------------------------------------------------------------------
CREATE TABLE alliance_buildings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    alliance_id UUID NOT NULL REFERENCES alliances(id) ON DELETE CASCADE,
    building_type VARCHAR(32) NOT NULL CHECK (building_type IN ('HQ', 'BANNER')),
    coord_x INT NOT NULL,
    coord_y INT NOT NULL,
    status VARCHAR(16) DEFAULT 'ACTIVE' CHECK (status IN ('UNDER_CONSTRUCTION', 'ACTIVE', 'DESTROYED')),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- -----------------------------------------------------------------------------
-- 9. EVENTOS GLOBALES Y CICLO CÍCLICO
-- -----------------------------------------------------------------------------
CREATE TABLE server_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    event_type VARCHAR(32) NOT NULL, -- 'CASTLE_BATTLE', 'SVS_WAR', 'BLIZZARD_SURVIVAL'
    phase VARCHAR(16) NOT NULL DEFAULT 'PREPARATION' CHECK (phase IN ('PREPARATION', 'EXECUTION', 'COOL_DOWN', 'ENDED')),
    start_time TIMESTAMP WITH TIME ZONE NOT NULL,
    end_time TIMESTAMP WITH TIME ZONE NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- -----------------------------------------------------------------------------
-- 10. ÍNDICES DE RENDIMIENTO Y OPTIMIZACIÓN
-- -----------------------------------------------------------------------------
CREATE INDEX idx_players_alliance ON players(alliance_id);
CREATE INDEX idx_players_coords ON players(city_x, city_y);
CREATE INDEX idx_marches_player ON marches(player_id);
CREATE INDEX idx_marches_state_arrival ON marches(state, arrival_time);
CREATE INDEX idx_marches_coords ON marches(target_x, target_y);
CREATE INDEX idx_alliance_buildings_coords ON alliance_buildings(coord_x, coord_y);
