import { Component, Injectable } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { SmartMeterMeasurements } from '../types/smartMeterMeasurement';


@Injectable()
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'Smart Meter SM_001';
  forecasts: SmartMeterMeasurements = [];

  constructor(private http: HttpClient) {
    http.get<SmartMeterMeasurements>('api/history/SM_001').subscribe({
      next: result => this.forecasts = result,
      error: console.error
    });
  }
}
