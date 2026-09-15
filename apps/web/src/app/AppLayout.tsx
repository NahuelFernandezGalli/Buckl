import { Link, NavLink, Outlet } from 'react-router'
import styles from './AppLayout.module.css'

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  isActive ? `${styles.navLink} ${styles.navLinkActive}` : styles.navLink

export function AppLayout() {
  return (
    <div className={styles.shell}>
      <header className={styles.header}>
        <Link to="/wardrobe" className={styles.brand}>
          Buckl
        </Link>
      </header>
      <nav aria-label="Main" className={styles.nav}>
        <NavLink to="/wardrobe" className={navLinkClass}>
          Wardrobe
        </NavLink>
        <NavLink to="/garments/new" className={navLinkClass}>
          Add garment
        </NavLink>
      </nav>
      <main className={styles.main}>
        <Outlet />
      </main>
    </div>
  )
}
