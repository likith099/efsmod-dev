const { createServer } = require("http");
const { parse } = require("url");
const next = require("next");

const dev = process.env.NODE_ENV !== "production";
// On Azure App Service, PORT can be a Windows named pipe under iisnode
const hostname = dev ? "localhost" : "0.0.0.0";
const port = process.env.PORT || 3000;
const isPipe = typeof port === "string" && port.startsWith("\\\\.\\pipe\\");

// when using middleware `hostname` and `port` must be provided below
const app = next({ dev, hostname, port });
const handle = app.getRequestHandler();

app.prepare().then(() => {
  createServer(async (req, res) => {
    try {
      // Be sure to pass `true` as the second argument to `url.parse`.
      // This tells it to parse the query portion of the URL.
      const parsedUrl = parse(req.url, true);
      const { pathname, query } = parsedUrl;

      await handle(req, res, parsedUrl);
    } catch (err) {
      console.error("Error occurred handling", req.url, err);
      res.statusCode = 500;
      res.end("internal server error");
    }
  }).listen(
    // If using iisnode named pipe, do not pass hostname
    isPipe ? port : Number(port),
    isPipe ? undefined : hostname,
    (err) => {
      if (err) throw err;
      if (isPipe) {
        console.log(`> Ready on named pipe ${port}`);
      } else {
        console.log(`> Ready on http://${hostname}:${port}`);
      }
    }
  );
});
