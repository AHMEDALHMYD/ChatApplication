import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { User } from '../models/user.model';
import { SignalRService } from './signalr.service';
import { environment } from '../environments/environment';


@Injectable({
  providedIn: 'root'
})
export class UserStatusService {
  private userStatusSubject = new BehaviorSubject<Map<string, boolean>>(new Map());
  public userStatus$ = this.userStatusSubject.asObservable();

  private onlineUsersSubject = new BehaviorSubject<User[]>([]);
  public onlineUsers$ = this.onlineUsersSubject.asObservable();

  private apiUrl = `${environment.apiUrl}/api/users`;     //'/api/users';      //'http://localhost:5000/api/users';

  constructor(
    private http: HttpClient,
    private signalRService: SignalRService
  ) {
    this.setupStatusListeners();
  }

  private setupStatusListeners(): void {
    this.signalRService.on('UserStatusChanged', (userId: string, isOnline: boolean) => {
      const userStatus = this.userStatusSubject.value;
      userStatus.set(userId, isOnline);
      this.userStatusSubject.next(new Map(userStatus));
      this.recalculateOnlineUsers();
    });
  }

  getChatUsers() {
    return this.http.get<User[]>(`${environment.apiUrl}/api/users/chats`);
  }



  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(this.apiUrl);
  }

  getUserStatus(userId: string): boolean {
    return this.userStatusSubject.value.get(userId) || false;
  }

  private recalculateOnlineUsers(): void {
    this.http.get<User[]>(this.apiUrl).subscribe(users => {
      const statusMap = this.userStatusSubject.value;
      const online = users.filter(u => statusMap.get(u.id));
      this.onlineUsersSubject.next(online);
    });
  }
}
