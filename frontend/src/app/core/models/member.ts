/** Metadata of one stored adventure picture. Bytes come from the image endpoint. */
export interface AdventureImage {
  readonly id: number;
  readonly title: string;
  readonly contentType: string;
  readonly sortOrder: number;
}

/** One card in the adventure list. */
export interface AdventureListItem {
  readonly id: number;
  readonly title: string;

  /** ISO timestamp. */
  readonly travelDate: string;

  readonly cost: number;

  /** Points awarded for completing the adventure. */
  readonly points: number;

  /** First image by sort order; null if the adventure has none. */
  readonly coverImageId: number | null;

  /** True while the travel date is still in the future. */
  readonly isUpcoming: boolean;
}

/** The detail page: everything above, plus the description and every picture. */
export interface AdventureDetail extends AdventureListItem {
  /** What the adventure involves. Empty when none was written. */
  readonly description: string;

  readonly images: readonly AdventureImage[];
}

/** One entry in the season picker. */
export interface Season {
  /** Round-tripped to the API, e.g. `2025-summer`. */
  readonly key: string;

  /** `spring` | `summer` | `autumn` | `winter` — translated by the client. */
  readonly name: string;

  /** The year the season starts in; winter runs into the next. */
  readonly year: number;

  /** English label, used only if a translation is missing. */
  readonly label: string;

  readonly isCurrent: boolean;
}

export interface LeaderboardEntry {
  readonly userId: number;
  readonly displayName: string;

  /** Null when the member has no Telegram picture — show initials. */
  readonly photoUrl: string | null;

  /** Points earned inside the selected season only. */
  readonly points: number;

  /** 1-based; ties share a place. */
  readonly rank: number;

  readonly isCurrentUser: boolean;
}

export interface Leaderboard {
  readonly seasons: readonly Season[];
  readonly selectedSeasonKey: string;
  readonly entries: readonly LeaderboardEntry[];
}

/** One adventure the member has taken part in. */
export interface ProfileHistoryItem {
  readonly transactionId: number;
  readonly adventureId: number | null;
  readonly title: string;

  /** ISO timestamp of the adventure. */
  readonly date: string;

  readonly coverImageId: number | null;

  /** Points this entry awarded. */
  readonly points: number;
}

export interface Profile {
  readonly tgUserId: number;
  readonly displayName: string;

  /** Telegram @username without the @; empty when unset. */
  readonly userName: string;

  readonly photoUrl: string | null;

  /** Current balance. */
  readonly points: number;

  /** Adventures the member has been credited for. */
  readonly travelsCount: number;

  readonly registeredAt: string;

  readonly history: readonly ProfileHistoryItem[];
}
