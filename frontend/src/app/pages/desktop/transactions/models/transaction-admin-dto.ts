export interface TransactionAdminDto {
  readonly id: number;

  readonly userId: number;
  readonly userName: string;
  readonly tgUserId: number;

  /** Null when the transaction is a manual points adjustment. */
  readonly travelId: number | null;
  readonly travelTitle: string;

  /** Signed points moved on the user's balance. */
  readonly points: number;

  readonly createdAt: string;

  /** Soft-delete flag. `false` means the transaction has been voided. */
  readonly status: boolean;
}

/** Body accepted by the admin create and update endpoints. */
export interface TransactionPayload {
  readonly userId: number;

  /** Null for a manual points adjustment. */
  readonly travelId: number | null;

  /** Signed points: positive adds to the balance, negative subtracts. */
  readonly points: number;

  readonly status: boolean;
}
