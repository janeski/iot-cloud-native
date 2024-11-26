export interface SmartMeterMeasurement {
    time: Date;
    measurement: number;
    smartMeterName: string;
    guid: string;
}

export type SmartMeterMeasurements = SmartMeterMeasurement[];