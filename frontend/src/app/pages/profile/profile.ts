import { Component, computed, inject, output, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { TranslatePipe } from '@ngx-translate/core';

import { MemberApi } from '../../core/services/member-api';
import { AuthService } from '../../core/services/auth-service';
import {
  APP_LANGUAGES,
  AppLanguage,
  LANGUAGE_NAMES,
  LanguageService,
} from '../../core/services/language-service';
import { TelegramService } from '../../core/services/telegram-service';
import { AdventureListItem } from '../../core/models/member';
import { Avatar } from '../../shared/components/avatar/avatar';
import { AdventuresList } from '../home/adventures-list/adventures-list';

/**
 * The member's own page: who they are, what they have earned, every adventure
 * they have been credited for — and the language the bot and the app use with
 * them.
 */
@Component({
  selector: 'app-profile',
  imports: [Avatar, AdventuresList, TranslatePipe],
  templateUrl: './profile.html',
  styleUrl: './profile.scss',
})
export class Profile {
  private readonly memberApi = inject(MemberApi);
  private readonly auth = inject(AuthService);
  private readonly telegram = inject(TelegramService);

  /** Lets a member jump from their history straight into the adventure. */
  readonly open = output<number>();

  private readonly resource = rxResource({
    stream: () => this.memberApi.getProfile(),
  });

  protected readonly profile = this.resource.value;
  protected readonly isLoading = this.resource.isLoading;
  protected readonly failed = computed(() => !!this.resource.error());

  protected readonly languages = APP_LANGUAGES.map(code => ({
    code,
    label: LANGUAGE_NAMES[code],
  }));

  protected readonly currentLanguage = inject(LanguageService).language;
  protected readonly savingLanguage = signal(false);
  protected readonly languageFailed = signal(false);

  /**
   * The history is rendered with the same card as the adventure list, so it is
   * reshaped into that list's item rather than given a second, near-identical
   * component. Nothing here is upcoming — it has all already happened.
   */
  protected readonly history = computed<readonly AdventureListItem[]>(() =>
    (this.profile()?.history ?? []).map(item => ({
      id: item.adventureId ?? 0,
      title: item.title,
      travelDate: item.date,
      cost: 0,
      points: item.points,
      coverImageId: item.coverImageId,
      isUpcoming: false,
    })),
  );

  protected retry(): void {
    this.resource.reload();
  }

  protected onOpen(adventureId: number): void {
    // A history row for a deleted adventure has no id to open.
    if (adventureId > 0) {
      this.open.emit(adventureId);
    }
  }

  protected async chooseLanguage(language: AppLanguage): Promise<void> {
    if (language === this.currentLanguage() || this.savingLanguage()) {
      return;
    }

    this.savingLanguage.set(true);
    this.languageFailed.set(false);

    try {
      await this.auth.changeLanguage(language);

      this.telegram.tapFeedback();
    } catch {
      this.languageFailed.set(true);
    } finally {
      this.savingLanguage.set(false);
    }
  }
}
