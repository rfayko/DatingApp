import { Component, inject, OnInit } from '@angular/core';
import { MessageService } from '../_services/message.service';
import { PageChangedEvent, PaginationModule } from 'ngx-bootstrap/pagination';
import { FormsModule } from '@angular/forms';
import { ButtonsModule } from 'ngx-bootstrap/buttons';
import { TimeagoModule } from 'ngx-timeago';
import { Message } from '../_models/message';
import { RouterLink } from '@angular/router';
import { TitleCasePipe } from '@angular/common';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [FormsModule, PaginationModule, ButtonsModule, TimeagoModule, RouterLink, TitleCasePipe],
  templateUrl: './messages.component.html',
  styleUrl: './messages.component.css'
})
export class MessagesComponent implements OnInit {
  messageService = inject(MessageService);
  accountService = inject(AccountService);
  container = "Inbox";
  pageNumber = 1;
  pageSize = 5;
  isOutbox = this.container === 'Outbox';

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages() {
    this.messageService.getMessages(this.pageNumber, this.pageSize, this.container);
  }

  pageChanged(event: PageChangedEvent) {
    if (this.pageNumber != event.page) {
      this.pageNumber = event.page;
      this.loadMessages();
    }
  }

  getRoute(message: Message) {
    let route = "";
    if (this.container === "Outbox") route = `/members/${message.recipientUsername}`;
    else route = `/members/${message.senderUsername}`;
    return route;
  }

  deleteMessage(id: number) {
    this.messageService.deleteMessage(id).subscribe({
      next: _ => {
        this.messageService.paginatedResult.update(prev => {
          if (prev && prev.items) {
            prev.items.splice(prev.items.findIndex(x => x.id == id), 1);
            return prev;
          } else {
            return prev;
          }
        })
      }
    });
  }
}
