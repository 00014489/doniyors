import { Component, computed, inject } from '@angular/core';
import { TelegramService } from '../../core/services/telegram-service';

@Component({
  selector: 'app-desktop-component',
  imports: [],
  templateUrl: './desktop-component.html',
  styleUrl: './desktop-component.scss',
})
export class DesktopComponent {
  private telegram = inject(TelegramService);

  readonly message = computed(() => {
    const user = this.telegram.user();
    return user ? `Hello, you are logged in with phone ${user.first_name}` : '';
  });
}
