# Progressive Overload Calculator

A minimal, dependency-free progressive overload calculator designed for fast use on a phone.

## What it does

- Calculates the next workout target from your current weight, reps, and sets
- Supports four simple overload strategies:
  - Add weight
  - Add reps
  - Add percentage
  - Double progression
- Works as a static site with no build step
- Includes a basic web app manifest and service worker so it can be added to a phone home screen

## Local use

Because the app is fully static, you can open `index.html` directly in a browser.

For the best service worker/PWA behavior, serve the repository with any simple static file server, for example:

```bash
python3 -m http.server 8080
```

Then open <http://localhost:8080>.

## Deploy to GitHub Pages

1. Push this branch.
2. In GitHub, go to **Settings → Pages**.
3. Under **Build and deployment**, choose **Deploy from a branch**.
4. Select your branch and the **/(root)** folder.
5. Save, then open the published Pages URL on your phone.
6. Use your browser’s **Add to Home Screen** option for quick access.

## Files

- `index.html` — single-page calculator UI
- `styles.css` — mobile-first styles
- `app.js` — calculator logic and DOM behavior
- `app.test.js` — lightweight Node tests for the core calculator logic
- `manifest.webmanifest` and `service-worker.js` — basic PWA support
