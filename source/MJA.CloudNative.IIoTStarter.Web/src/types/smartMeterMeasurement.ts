export interface SmartMeterMeasurement {
    date: string;
    temperatureC: number;
    temperatureF: number;
    summary: string;
}

export type SmartMeterMeasurements = SmartMeterMeasurement[];