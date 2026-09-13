export interface TravelImageDto {
  readonly id: number;
  readonly title: string;
  readonly contentType: string;
  readonly sortOrder: number;
}

export interface TravelAdminDto {
  readonly id: number;
  readonly title: string;

  /** Shown on the adventure's page in the member app. May be empty. */
  readonly description: string;
  readonly travelDate: string;
  readonly cost: number;
  readonly points: number;
  readonly createdAt: string;

  /** Soft-delete flag. `false` means the adventure is disabled. */
  readonly status: boolean;

  /** Ordered images; the first one is the cover shown in the list. */
  readonly images: readonly TravelImageDto[];
}

/** Scalar fields of the adventure form. */
export interface AdventurePayload {
  readonly title: string;
  readonly description: string;
  readonly travelDate: string;
  readonly cost: number;
  readonly points: number;
  readonly status: boolean;
}

/**
 * What the adventure form produces: the scalar fields plus the images to
 * upload and the already stored images to keep.
 */
export interface AdventureSubmit {
  readonly adventure: AdventurePayload;
  readonly newImages: readonly File[];
  readonly keepImageIds: readonly number[];
}

/** An adventure must carry between one and five images. */
export const MIN_ADVENTURE_IMAGES = 1;
export const MAX_ADVENTURE_IMAGES = 5;
