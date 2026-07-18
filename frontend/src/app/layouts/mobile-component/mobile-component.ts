import { Component, computed, inject } from '@angular/core';
import { TelegramService } from '../../core/services/telegram-service';

@Component({
  selector: 'app-mobile-component',
  imports: [],
  templateUrl: './mobile-component.html',
  styleUrl: './mobile-component.scss',
})
export class MobileComponent {
  private telegram = inject(TelegramService);

  readonly message = computed(() => {
    const user = this.telegram.user();
    return user ? `Hello, you are logged in with phone ${user.first_name}` : '';
  });
}
