import { Component, OnInit, Input } from '@angular/core';
import { Message } from 'src/app/_models/message';
import { UserService } from 'src/app/_services/user.service';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from 'src/app/_services/auth.service';
import { tap } from 'rxjs/operators';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  @Input() recipientId: number;
  messages: Message[];
  newMessage: any = {};

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private alertify: AlertifyService,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    this.loadMessages();
  }

  loadMessages() {
    // Add + to coerce to number type
    const currentUserId = +this.authService.decodedToken.nameid;
    this.userService.getMessageThread(this.authService.decodedToken.nameid, this.recipientId)
      .pipe(
        tap(messages => {
          // console.log('trying to loop through messages #:', messages.length , ' for user #', currentUserId);
          for (let i = 0; i < messages.length; ++i) {
            if (messages[i].isRead === false && messages[i].recipientId === currentUserId) {
              // console.log('mark message as read:', messages[i]);
              this.userService.markMessageAsRead(messages[i].id, currentUserId);
            }
          }
        })
      )
      .subscribe(messages => {
        this.messages = messages;
      }, error => {
        this.alertify.error(error);
      });
  }

  sendMessage() {
    this.newMessage.recipientId = this.recipientId;
    this.userService.sendMessage(this.authService.decodedToken.nameid, this.newMessage)
      .subscribe((msg: Message) => {
        // debugger;
        this.messages.unshift(msg);
        this.newMessage.content = '';
      }, error => {
        this.alertify.error(error);
      });
  }

}
