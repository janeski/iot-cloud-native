import { EventEmitter, Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({
    providedIn: 'root'
  })
  export class SignalRService {
    private hubConnection: signalR.HubConnection;
    public messageReceived = new EventEmitter<any>();


    constructor() {
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl('/iot')
        .build();
      this.hubConnection.on('ReceiveMqttMessage', (user, message) => {
       this.messageReceived.emit(message);
      });
      this.hubConnection.start()
        .catch(err => console.error(err));
    }
    sendMessage(user: string, message: string): void {
      this.hubConnection.invoke('SendMqttMessage', user, message)
        .catch(err => console.error(err));
    }
  }