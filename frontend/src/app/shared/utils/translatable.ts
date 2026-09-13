/**
 * A translation key with its parameters. Kept unresolved and translated in the
 * template, so a message already on screen follows a language change.
 */
export interface Translatable {
  readonly key: string;
  readonly params?: Record<string, unknown>;
}
