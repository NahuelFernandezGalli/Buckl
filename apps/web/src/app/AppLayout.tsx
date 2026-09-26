import { Link, NavLink, Outlet } from 'react-router'
import { Button } from '../components/Button/Button'
import { useSession } from '../session/useSession'
import styles from './AppLayout.module.css'

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  isActive ? `${styles.navLink} ${styles.navLinkActive}` : styles.navLink

export function AppLayout() {
  const session = useSession()
  const displayName = session.state.status === 'signedIn' ? session.state.user.displayName : null

  return (
    <div className={styles.shell}>
      <header className={styles.header}>
        <Link to="/wardrobe" className={styles.brand}>
          Buckl
        </Link>
        <div className={styles.account}>
          {displayName && <span className={styles.user}>{displayName}</span>}
          <Button variant="secondary" onClick={() => void session.signOut()}>
            Log out
          </Button>
        </div>
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
