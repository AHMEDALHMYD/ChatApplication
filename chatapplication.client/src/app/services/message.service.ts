import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { Message } from '../models/message.model';
import { SignalRService } from './signalr.service';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private messagesSubject = new BehaviorSubject<Message[]>([]);
  public messages$ = this.messagesSubject.asObservable();

  private typingUsersSubject = new BehaviorSubject<Map<string, string>>(new Map());
  public typingUsers$ = this.typingUsersSubject.asObservable();

  private apiUrl = `${environment.apiUrl}/api/messages`;

  constructor(
    private http: HttpClient,
    private signalRService: SignalRService
  ) {}

  private setupMessageListeners(): void {
    this.signalRService.on('ReceiveMessage', (message: Message) => {
      const currentMessages = this.messagesSubject.value;
      this.messagesSubject.next([...currentMessages, message]);
    });
  }

  sendMessage(receiverId: string, content: string): void {
    this.signalRService.invoke('SendMessage', receiverId, content)
      .catch(err => console.error('Send Message Error: ', err));
  }

  loadMessages(userId: string): Observable<Message[]> {
    return this.http.get<Message[]>(`${this.apiUrl}/${userId}`);
  }


  setMessages(messages: Message[]): void {
    this.messagesSubject.next(messages);
  }

  clearMessages(): void {
    this.messagesSubject.next([]);
  }

  public initSignalRListeners(): void {
  this.setupMessageListeners();
}
}
