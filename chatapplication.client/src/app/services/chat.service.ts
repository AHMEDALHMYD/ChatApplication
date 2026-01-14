import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { SignalRService } from './signalr.service';
import { MessageService } from './message.service';
import { UserStatusService } from './user-status.service';
import { AuthService } from './auth.service';
import { User } from '../models/user.model';
import { environment } from '../environments/environment';


@Injectable({
  providedIn: 'root'
})
export class ChatService {

  constructor(
    private signalRService: SignalRService,
    private messageService: MessageService,
    private userStatusService: UserStatusService,
    private authService: AuthService,
    private http: HttpClient
  ) { }

  async startConnection(): Promise<void> {
    const token = this.authService.getToken();
    if (!token) throw new Error('No authentication token');

    await this.signalRService.startConnection(token);
    this.messageService.initSignalRListeners();
  }

  stopConnection(): void {
    this.signalRService.stopConnection();
  }

  // Messages
  get messages$() {
    return this.messageService.messages$;
  }

  sendMessage(receiverId: string, content: string): void {
    this.messageService.sendMessage(receiverId, content);
  }

  loadMessages(userId: string) {
    return this.messageService.loadMessages(userId);
  }

  setMessages(messages: any[]): void {
    this.messageService.setMessages(messages);
  }

  clearMessages(): void {
    this.messageService.clearMessages();
  }

  // User status
  get userStatus$() {
    return this.userStatusService.userStatus$;
  }

  getChatUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${environment.apiUrl}/api/users/chats`);
  }
}
