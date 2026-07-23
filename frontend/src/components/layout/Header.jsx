import {
  useEffect,
  useRef,
  useState,
} from 'react';
import {
  useNavigate,
} from 'react-router-dom';
import {
  useAuth,
  ROLE_OPTIONS,
} from '../../context/AuthContext';
import {
  notificationService,
} from '../../services/services';
import Avatar from '../ui/Avatar';

export default function Header({
  title,
  subtitle,
  onMenuToggle,
}) {
  const {
    user,
    logout,
  } = useAuth();

  const [userOpen, setUserOpen] =
    useState(false);
  const [
    notificationsOpen,
    setNotificationsOpen,
  ] = useState(false);
  const [
    notifications,
    setNotifications,
  ] = useState([]);
  const [unreadCount, setUnreadCount] =
    useState(0);

  const userRef = useRef(null);
  const notificationRef =
    useRef(null);
  const navigate = useNavigate();

  const roleLabel =
    ROLE_OPTIONS.find(
      (role) =>
        role.value === user?.role,
    )?.label || user?.role;

  const loadNotifications =
    async () => {
      if (!user) return;

      try {
        const [
          items,
          count,
        ] = await Promise.all([
          notificationService.list(),
          notificationService
            .unreadCount(),
        ]);

        setNotifications(items);
        setUnreadCount(count.count);
      } catch {
        // Header notifications should not block the page.
      }
    };

  useEffect(() => {
    loadNotifications();

    const timer = window.setInterval(
      loadNotifications,
      60000,
    );

    return () =>
      window.clearInterval(timer);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [user?.id]);

  useEffect(() => {
    const onClick = (event) => {
      if (
        userRef.current &&
        !userRef.current.contains(
          event.target,
        )
      ) {
        setUserOpen(false);
      }

      if (
        notificationRef.current &&
        !notificationRef.current.contains(
          event.target,
        )
      ) {
        setNotificationsOpen(false);
      }
    };

    document.addEventListener(
      'mousedown',
      onClick,
    );

    return () =>
      document.removeEventListener(
        'mousedown',
        onClick,
      );
  }, []);

  const openNotifications =
    async () => {
      setUserOpen(false);

      const next =
        !notificationsOpen;

      setNotificationsOpen(next);

      if (next) {
        await loadNotifications();
      }
    };

  const markRead =
    async (notification) => {
      if (!notification.isRead) {
        try {
          await notificationService
            .markRead(
              notification.id,
            );

          setNotifications(
            (items) =>
              items.map((item) =>
                item.id ===
                notification.id
                  ? {
                      ...item,
                      isRead: true,
                    }
                  : item,
              ),
          );

          setUnreadCount(
            (count) =>
              Math.max(0, count - 1),
          );
        } catch {
          // Keep the notification visible if the request fails.
        }
      }
    };

  const markAllRead =
    async () => {
      try {
        await notificationService
          .markAllRead();

        setNotifications(
          (items) =>
            items.map((item) => ({
              ...item,
              isRead: true,
            })),
        );

        setUnreadCount(0);
      } catch {
        // Keep current state when the API request fails.
      }
    };

  return (
    <header className="header">
      <div className="header-left">
        <button
          className="menu-toggle"
          onClick={onMenuToggle}
          aria-label="Menu"
        >
          ☰
        </button>

        <div>
          <h1>{title}</h1>

          {subtitle && (
            <p
              className="text-sm text-muted"
              style={{ margin: 0 }}
            >
              {subtitle}
            </p>
          )}
        </div>
      </div>

      <div className="header-right">
        <div
          className="dropdown"
          ref={notificationRef}
        >
          <button
            type="button"
            aria-label="Notifications"
            onClick={openNotifications}
            style={{
              position: 'relative',
              border:
                '1px solid var(--color-border)',
              background:
                'var(--color-surface)',
              borderRadius:
                '999px',
              width: 42,
              height: 42,
              cursor: 'pointer',
              fontSize: '1.15rem',
            }}
          >
            🔔

            {unreadCount > 0 && (
              <span
                style={{
                  position:
                    'absolute',
                  top: -5,
                  right: -5,
                  minWidth: 19,
                  height: 19,
                  padding: '0 5px',
                  display: 'grid',
                  placeItems:
                    'center',
                  borderRadius:
                    999,
                  background:
                    '#dc2626',
                  color: '#fff',
                  fontSize:
                    '0.68rem',
                  fontWeight: 800,
                }}
              >
                {unreadCount > 99
                  ? '99+'
                  : unreadCount}
              </span>
            )}
          </button>

          {notificationsOpen && (
            <div
              className="dropdown-menu"
              style={{
                width: 360,
                maxWidth:
                  'calc(100vw - 2rem)',
                maxHeight: 480,
                overflowY: 'auto',
              }}
            >
              <div
                className="flex items-center justify-between"
                style={{
                  padding:
                    '0.75rem 1rem',
                  borderBottom:
                    '1px solid var(--color-border)',
                }}
              >
                <strong>
                  Notifications
                </strong>

                {unreadCount > 0 && (
                  <button
                    type="button"
                    onClick={markAllRead}
                    style={{
                      border: 0,
                      background:
                        'none',
                      color:
                        'var(--color-primary)',
                      cursor:
                        'pointer',
                      fontSize:
                        '0.8rem',
                    }}
                  >
                    Mark all read
                  </button>
                )}
              </div>

              {notifications.length ===
              0 ? (
                <div
                  className="text-sm text-muted"
                  style={{
                    padding: '1rem',
                  }}
                >
                  No notifications yet.
                </div>
              ) : (
                notifications
                  .slice(0, 12)
                  .map(
                    (notification) => (
                      <button
                        type="button"
                        key={
                          notification.id
                        }
                        onClick={() =>
                          markRead(
                            notification,
                          )
                        }
                        style={{
                          width: '100%',
                          border: 0,
                          borderBottom:
                            '1px solid var(--color-border)',
                          background:
                            notification.isRead
                              ? 'var(--color-surface)'
                              : 'rgba(37, 99, 235, 0.07)',
                          textAlign:
                            'left',
                          padding:
                            '0.85rem 1rem',
                          cursor:
                            'pointer',
                        }}
                      >
                        <div
                          className="flex items-center justify-between gap-2"
                        >
                          <strong
                            style={{
                              fontSize:
                                '0.9rem',
                            }}
                          >
                            {
                              notification.title
                            }
                          </strong>

                          {!notification.isRead && (
                            <span
                              style={{
                                width: 8,
                                height: 8,
                                borderRadius:
                                  '50%',
                                background:
                                  'var(--color-primary)',
                                flexShrink:
                                  0,
                              }}
                            />
                          )}
                        </div>

                        <div className="text-sm text-secondary">
                          {
                            notification.message
                          }
                        </div>

                        <div className="text-sm text-muted">
                          {
                            notification.createdAgo
                          }
                        </div>
                      </button>
                    ),
                  )
              )}
            </div>
          )}
        </div>

        <div
          className="dropdown"
          ref={userRef}
        >
          <div
            className="user-chip"
            onClick={() => {
              setNotificationsOpen(
                false,
              );

              setUserOpen(
                (value) => !value,
              );
            }}
          >
            <Avatar
              name={user?.name}
              size="sm"
            />

            <div className="user-chip-info">
              <span className="user-chip-name">
                {user?.name}
              </span>

              <span className="user-chip-role">
                {roleLabel}
              </span>
            </div>

            <span
              style={{
                color:
                  'var(--color-text-muted)',
              }}
            >
              ▾
            </span>
          </div>

          {userOpen && (
            <div className="dropdown-menu">
              <div
                style={{
                  padding:
                    '0.75rem 1rem',
                  borderBottom:
                    '1px solid var(--color-border)',
                }}
              >
                <div
                  style={{
                    fontWeight: 700,
                  }}
                >
                  {user?.name}
                </div>

                <div className="text-sm text-muted">
                  {user?.email}
                </div>
              </div>

              <button
                className="dropdown-item"
                onClick={() => {
                  setUserOpen(false);
                  navigate('/profile');
                }}
              >
                👤 My Profile
              </button>

              <button
                className="dropdown-item danger"
                onClick={() => {
                  logout();
                  navigate('/login');
                }}
              >
                🚪 Sign Out
              </button>
            </div>
          )}
        </div>
      </div>
    </header>
  );
}
