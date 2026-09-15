export class GarmentError extends Error {
  readonly code: string

  constructor(code: string, message: string) {
    super(message)
    this.name = new.target.name
    this.code = code
  }
}

export class GarmentNotFoundError extends GarmentError {
  constructor(id: string) {
    super('garment.not_found', `Garment ${id} was not found.`)
  }
}

export class ArchivedGarmentIsReadOnlyError extends GarmentError {
  constructor(id: string) {
    super('garment.archived_read_only', `Garment ${id} is archived and cannot be edited.`)
  }
}

export class GarmentAlreadyArchivedError extends GarmentError {
  constructor(id: string) {
    super('garment.already_archived', `Garment ${id} is already archived.`)
  }
}

export class GarmentNotArchivedError extends GarmentError {
  constructor(id: string) {
    super('garment.not_archived', `Garment ${id} is not archived.`)
  }
}
