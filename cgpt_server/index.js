// index.js

const express = require('express');
const app = express();
const bodyParser = require('body-parser');
const contentGenerator = require('./contentGenerator');

const port = process.env.PORT || 3000;

// Middleware
app.use(bodyParser.urlencoded({ extended: false }));
app.use(bodyParser.json());

// Routes
app.get('/terrainContent/:terrainType', async (req, res) => {
  try {
    const terrainType = req.params.terrainType;
    const content = await contentGenerator.generateTerrainContent(terrainType);
    res.json(content);
  } catch (err) {
    console.error(err);
    res.status(500).send('Internal Server Error');
  }
});

// Start server
app.listen(port, () => console.log(`Server listening on port ${port}`));
