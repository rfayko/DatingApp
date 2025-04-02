import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { PaginatedResult } from '../_models/pagination';
import { Message } from '../_models/message';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { User } from '../_models/user';
import { Group } from '../_models/group';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl;
  hubUrl = environment.hubsUrl;
  private http = inject(HttpClient);  
  hubConnection? : HubConnection;
  paginatedResult = signal<PaginatedResult<Message[]> | null>(null);
  messageThread = signal<Message[]>([]);

  // Hub services
  createHubConnection(user: User, otherUsername: string) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'message?user=' + otherUsername, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch(error => console.log(error));

    this.hubConnection.on('ReceivedMessageThread', messages => {
      this.messageThread.set(messages);
    });

    this.hubConnection.on('NewMessage', message => {
      this.messageThread.update(messages => [...messages, message]);
    });

    // This is awkward logic in my view.
    // I think the idea is that on a new connection to a group
    // there is a SignalR update from the server providing the 
    // updated group. If the message recipient (otherUsername) is online in the thread
    // and has any messages not read, mark them read here instead of from server so the 
    // sender sees them as read.
    this.hubConnection.on("UpdatedGroup", (group: Group) => {
      if (group.connections.some(c => c.username === otherUsername)) {
        this.messageThread.update(messages => {
          messages.forEach(m => {
            if (!m.dateRead) {
              m.dateRead = new Date(Date.now());
            }
          })
          return messages;
        })
      }
    });
  }

  stopHubConnection() {
    if (this.hubConnection?.state == HubConnectionState.Connected) {
      this.hubConnection.stop().catch(error => console.log(error));
    }
  }

  //Http Services
  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = setPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);

    return this.http.get<Message[]>(`${this.baseUrl}messages`, {observe: 'response', params}).subscribe({
      next: response => setPaginatedResponse(response, this.paginatedResult)
    });
  }

  getMessageThread(targetUsername: string) {
    return this.http.get<Message[]>(this.baseUrl + "messages/thread/" + targetUsername);    

  }

  async sendMessage(username: string, content: string) {
    // return this.http.post<Message>(this.baseUrl + "messages", {recipientUserName: username, content});

    return this.hubConnection?.invoke('SendMessage', {recipientUserName: username, content: content});
  }

  deleteMessage(id: number) {
    return this.http.delete<Message>(this.baseUrl + "messages/" + id);
  }


}
