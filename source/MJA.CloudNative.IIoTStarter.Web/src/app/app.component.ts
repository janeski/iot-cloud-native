import { Component, Injectable, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { SmartMeterMeasurements } from '../types/smartMeterMeasurement';
import Chart, { ChartConfiguration, ChartOptions } from 'chart.js/auto';
import { BaseChartDirective } from 'ng2-charts';

@Injectable()
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, BaseChartDirective],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {

  @ViewChild(BaseChartDirective)
  chart!: BaseChartDirective;
  
  smartMeterMeasurements: SmartMeterMeasurements = [];

  public lineChartData: ChartConfiguration<'line'>['data'] = {
    labels: [
      'January',
      'February',
      'March',
      'April',
      'May',
      'June',
      'July'
    ],
    datasets: [
      {
        data: [ 65, 59, 80, 81, 56, 55, 40 ],
        label: 'SM_001',
        fill: true,
        tension: 0.5,
        borderColor: 'black',
        backgroundColor: 'rgba(255,0,0,0.3)'
      }
    ]
  };
  public lineChartOptions: ChartOptions<'line'> = {
    responsive: false
  };
  public lineChartLegend = true;

  constructor(private http: HttpClient) {


  }

  ngOnInit(): void {
    this.http.get<SmartMeterMeasurements>('api/history/SM_001').subscribe({
      next: result => {
        this.smartMeterMeasurements = result;
        this.lineChartData.labels = this.smartMeterMeasurements.map(smartMeterMeasurement => new Date(smartMeterMeasurement.time).toLocaleTimeString('en-US', { hour: 'numeric', minute: 'numeric', second: 'numeric', hour12: false }));
        this.lineChartData.datasets[0].data = this.smartMeterMeasurements.map(smartMeterMeasurement => smartMeterMeasurement.measurement); 
        console.log(this.lineChartData.labels);
        this?.chart?.chart?.update();
      },
      error: console.error
    });
  }

}
