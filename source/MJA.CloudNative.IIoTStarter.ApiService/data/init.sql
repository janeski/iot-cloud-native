-- Postgres init script

-- Create the smart_meter_measurement table
CREATE TABLE IF NOT EXISTS smart_meter_measurement 
(
     id UUID NOT NULL,
     smartmetername TEXT NOT NULL,
     time TIMESTAMP WITH TIME ZONE NOT NULL,
     measurement DOUBLE PRECISION NOT NULL
);

CREATE EXTENSION IF NOT EXISTS timescaledb CASCADE;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
SELECT create_hypertable('smart_meter_measurement', by_range('time'));

-- Insert random sinusoidal values into smart_meter_measurement table
INSERT INTO smart_meter_measurement (id, smartmetername, time, measurement)
SELECT 
    uuid_generate_v4(),                                   -- Generate a random UUID
    'SM_001',                                       -- Use a fixed SmartMeter name
    gs AT TIME ZONE 'UTC' AS Timestamp,                  -- Generate timestamps with time zone
    10 * SIN(2 * PI() * 2 * EXTRACT(HOUR FROM gs) / 24) + -- Sinusoidal value
    (RANDOM() - 0.5) * 0.5                                -- Add some noise
FROM 
    generate_series(
        NOW() - INTERVAL '7 days',  -- Start time
        NOW(),                      -- End time
        INTERVAL '1 minute'         -- Resolution
    ) AS gs;