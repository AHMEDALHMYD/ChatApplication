import { Component, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { ChatService } from '../../services/chat.service';
import { SignalRService } from '../../services/signalr.service';
import { MessageService } from '../../services/message.service';
import { UserStatusService } from '../../services/user-status.service';
import { User } from '../../models/user.model';
import { Message } from '../../models/message.model';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.css'
})
export class ChatComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('messagesContainer') private messagesContainer!: ElementRef;

  currentUser: User | null = null;
  onlineUsers: User[] = [];
  selectedUser: User | null = null;
  messages: Message[] = [];
  newMessage = '';
  chatUsers: User[] = [];
  allUsers: User[] = [];
  filteredUsers: User[] = [];
  searchTerm = '';

  private subscriptions: Subscription[] = [];
  private shouldScrollToBottom = false;
  showSearch = false;


  constructor(
    private authService: AuthService,
    private chatService: ChatService,
    private signalRService: SignalRService,
    private messageService: MessageService,
    private userStatusService: UserStatusService,
    private router: Router
  ) { }

  async ngOnInit() {

    this.currentUser = this.authService.getCurrentUser();
    if (!this.currentUser) {
      this.router.navigate(['/login']);
      return;
    }

    // Connect to SignalR

    const token = this.authService.getToken();
    if (!token) {
      this.router.navigate(['/login']);
      return;
    }

    await this.signalRService.startConnection(token);

    this.loadUsers();
    this.loadAllUsers();


    // Subscribe to online users
    this.subscriptions.push(
      this.userStatusService.onlineUsers$.subscribe(users => {
        this.onlineUsers = users.filter(u => u.id !== this.currentUser?.id);
      })
    );

    // Subscribe to new messages
    this.subscriptions.push(
      this.messageService.messages$.subscribe(messages => {
        if (!this.selectedUser || !this.currentUser) return;
        this.messages = messages.filter(m =>
          (m.senderId === this.currentUser!.id && m.receiverId === this.selectedUser!.id) ||
          (m.senderId === this.selectedUser!.id && m.receiverId === this.currentUser!.id)
        );

        this.shouldScrollToBottom = true;
      })
    );
    this.signalRService.on('ReceiveMessage', (message: Message) => {

    if (!this.currentUser || !this.selectedUser) return;

    const isCurrentChat =
      (message.senderId === this.currentUser.id &&
      message.receiverId === this.selectedUser.id) ||
      (message.senderId === this.selectedUser.id &&
      message.receiverId === this.currentUser.id);

    if (isCurrentChat) {
      this.messages.push(message);
      this.shouldScrollToBottom = true;
    }
    const exists = this.chatUsers.some(u => u.id === message.senderId);

    if (!exists) {
      this.authService.getUsers().subscribe(users => {
        const sender = users.find(u => u.id === message.senderId);
        if (sender) {
          this.chatUsers.unshift(sender);
          this.filteredUsers = [...this.chatUsers];
        }
      });
    }
  });

  }

  ngAfterViewChecked() {
    if (this.shouldScrollToBottom) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    this.signalRService.stopConnection();
  }

  async selectUser(user: User) {

    if (!this.chatUsers.find(u => u.id === user.id)) {
      this.chatUsers.unshift(user);
    }

    this.searchTerm = '';
    this.filteredUsers = [...this.chatUsers];

    this.selectedUser = user;
    this.messages = [];

    this.messageService.loadMessages(user.id)
      .subscribe(messages => {
        this.messageService.setMessages(messages);
        this.shouldScrollToBottom = true;
      });
  }

  async sendMessage() {

    if (!this.newMessage.trim() || !this.selectedUser) {
      return;
    }

    const messageContent = this.newMessage.trim();
    this.newMessage = '';

    this.chatService.sendMessage(
      this.selectedUser.id,
      messageContent
    );
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  formatTime(timestamp: Date): string {
    const date = new Date(timestamp);
    return date.toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  private scrollToBottom(): void {
    try {
      if (this.messagesContainer) {
        this.messagesContainer.nativeElement.scrollTop =
          this.messagesContainer.nativeElement.scrollHeight;
      }
    } catch (err) {
      console.error('Error scrolling to bottom:', err);
    }
  }
  loadUsers() {
    this.chatService.getChatUsers().subscribe(users => {
      this.chatUsers = users;
      this.filteredUsers = [...this.chatUsers];
    });
  }


  loadAllUsers() {
    this.authService.getUsers().subscribe(users => {
      this.allUsers = users.filter(u => u.id !== this.currentUser?.id);
    });
  }

  applyFilter() {
    const term = this.searchTerm.trim().toLowerCase();

    if (!term) {
      this.filteredUsers = [...this.chatUsers];
      return;
    }

    this.filteredUsers = this.allUsers.filter(u =>
      u.username.toLowerCase().includes(term)
    );
  }


}
