# Frontend Rules (React + Material-UI)

## Stack

- React 19 + Material-UI 7 (MUI)
- React Router v6 for routing
- Axios for API calls (centralized in `services/api.js`)
- Source in `src/MetalReleaseTracker.Frontend/`
- Built output goes to CoreDataService `wwwroot/`

## API Calls

- All API calls go through the centralized `api` instance in `services/api.js`
- JWT token is added via axios request interceptor — never add manually
- 401 responses trigger automatic logout and redirect to `/login`
- Query parameters are built with `URLSearchParams`
- Each exported function represents a single API call

## Component Patterns

- Use Material-UI components (Box, Card, Button, Typography, etc.)
- Dark theme by default (defined in App.js)
- Protected routes use `<ProtectedRoute>` wrapper component
- Routes are in App.js using `<Routes>` / `<Route>`
- Custom hooks for shared logic (`usePageMeta`, `useAuth`)

## SEO & Meta

- `usePageMeta(title, description, image)` hook sets `document.title` and og/twitter meta tags
- Title format: `{PageTitle} | Metal Release Tracker`
- Always set meta tags for pages with dynamic content

## Local Development

- `npm start` proxies to `localhost:5002` (configured in package.json proxy)
- Use `--legacy-peer-deps` flag for `npm install`
- For production build: `npm run build`

## UI Testing (Mandatory per CLAUDE.md)

- After any UI changes, build Docker and verify with Playwright
- Test both desktop and mobile (375x812) viewports
- Always verify interactive elements work (buttons, links, filters)
