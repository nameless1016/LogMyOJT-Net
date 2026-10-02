# LogMyOJT (Blazor + Tailwind)

Blazor Web App (.NET 8) styled with Tailwind CSS. No database yet, everything
you type only lives in memory for the session, so a browser refresh resets it.

Pages: Login (`/`), Register, Dashboard, Time logs, Placement, Reports,
Profile and Settings. On the Time logs page there's a "Load sample data"
button if you want to try the app without typing everything in.

## Run it

```
dotnet run
```

Then open the http link it prints in the terminal. The compiled stylesheet
(`wwwroot/css/tailwind.css`) is already in the repo, so you don't need Node
just to run the app.

## Changing the styles

Tailwind needs Node. Install it once, then rebuild the css after you change
any classes:

```
npm install
npm run css:build
```

Or leave `npm run css:watch` running while you work. Colors and fonts live in
`tailwind.config.js`, and the shared button/input/card classes are in
`Styles/input.css`.
