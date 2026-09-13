export interface UserAdminDto {
  readonly id: number;
  readonly tgUserId: number;
  readonly userName: string;
  readonly languageCode: string;
  /** Derived from the user's active transactions; read-only for the client. */
  readonly points: number;

  readonly typeUserId: number;
  readonly typeUserName: string;
  readonly registeredAt: string;
  readonly updatedAt: string;

  /** Soft-delete flag. `false` means the account is disabled. */
  readonly status: boolean;
}

/** Body accepted by the admin create and update endpoints. */
export interface UserPayload {
  readonly userName: string;
  readonly tgUserId: number;
  readonly languageCode: string;
  readonly typeUserId: number;
  readonly status: boolean;
}

/** Selectable account type (SuperAdmin / Admin / User). */
export interface UserRoleDto {
  readonly id: number;
  readonly name: string;
}
