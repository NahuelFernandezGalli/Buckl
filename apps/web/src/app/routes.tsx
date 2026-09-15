import { Navigate, type RouteObject } from 'react-router'
import { AddGarmentPage } from '../features/add-garment/AddGarmentPage'
import { EditGarmentPage } from '../features/edit-garment/EditGarmentPage'
import { GarmentDetailPage } from '../features/garment-detail/GarmentDetailPage'
import { WardrobePage } from '../features/wardrobe/WardrobePage'
import { AppLayout } from './AppLayout'
import { NotFoundPage } from './NotFoundPage'

export const routes: RouteObject[] = [
  {
    path: '/',
    element: <AppLayout />,
    children: [
      { index: true, element: <Navigate to="/wardrobe" replace /> },
      { path: 'wardrobe', element: <WardrobePage /> },
      { path: 'wardrobe/:garmentId', element: <GarmentDetailPage /> },
      { path: 'wardrobe/:garmentId/edit', element: <EditGarmentPage /> },
      { path: 'garments/new', element: <AddGarmentPage /> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
]
