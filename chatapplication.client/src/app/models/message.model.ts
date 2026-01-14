export interface Message {
  id: string;
  senderId: string;
  senderUsername: string;
  receiverId: string;
  content: string;
  timestamp: Date;
  isRead: boolean;
}
