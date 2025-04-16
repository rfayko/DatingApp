
export interface Photo {
  id: number
  url: string
  isMain: boolean
  isApproved: boolean
}

export interface PhotoToModerate {
  id: number
  url: string
  username: string
  isApproved: boolean
}

