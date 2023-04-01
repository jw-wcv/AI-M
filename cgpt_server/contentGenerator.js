// OpenAI GPT-3 configuration
const { Configuration, OpenAIApi } = require("openai");
const configuration = new Configuration({
    apiKey: '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',
  });
const openai = new OpenAIApi(configuration);

const {axios} = require('axios');
const { count } = require("console");

const noise = require('noisejs');

let zoneData = {
  zoneName: null,
  zoneDescription: null
};

// Mapping of location coordinates to terrain types
let locationToTerrainType = {
  "forest": [
    {"x": 500, "y": 500, "terrain": "forest"},
    {"x": 750, "y": 750, "terrain": "river"},
    {"x": 250, "y": 750, "terrain": "mountain"}
  ],
  "mountain": [
    {"x": 250, "y": 250, "terrain": "mountain"},
    {"x": 750, "y": 250, "terrain": "forest"},
    {"x": 500, "y": 750, "terrain": "desert"}
  ],
  "desert": [
    {"x": 750, "y": 500, "terrain": "desert"},
    {"x": 250, "y": 500, "terrain": "mountain"},
    {"x": 500, "y": 250, "terrain": "river"}
  ],
  "town": [
    {"x": 750, "y": 500, "terrain": "town"},
    {"x": 250, "y": 500, "terrain": "town"},
    {"x": 500, "y": 250, "terrain": "town"}
  ],
  "river": [
    {"x": 250, "y": 750, "terrain": "mountain"},
    {"x": 750, "y": 750, "terrain": "river"},
    {"x": 500, "y": 250, "terrain": "river"}
  ]
};

// Returns the terrain type for a given location
// Use this prior to using the generateContent method by using the state of the player to understand their location and a given region 
function getTerrainType(x, y) {
  let terrainType = null;
  Object.keys(locationToTerrainType).forEach((region) => {
    locationToTerrainType[region].forEach((mapping) => {
      if (mapping.x === x && mapping.y === y) {
        terrainType = mapping.terrain;
      }
    });
  });
  if (terrainType === null) {
    throw new Error(`Unknown location (${x}, ${y})`);
  }
  return terrainType;
}

// Invoke functions for generating content based on terrain type and return to client 
async function generateTerrainContent(terrainType) {
  // Generate content based on terrain type and return to client
  const content = {
    "terrain": terrainType,
    "locationTerrain": await generateLocationTerrain(terrainType),
    "locationMap": await generateNoiseMap(terrainType)
    //"npcs": await generateNPCs(terrainType),
    //"quests": await generateQuests(terrainType)
    //"resources": await generateResources(terrainType)
  };
  return content;
}

//generate terrain details for the zone 
async function generateLocationTerrain(location) {
  try {
    const terrainType = location;
      
    let terrain = {};
    let description = '';
    let terrainDetails = [];

    let terrainPrompt = `Generate a name and description for a zone with terrain of type ${terrainType} and coordinates ${location.x},${location.y}. Exclusively use the following JSON format for the response: 
[
  {
    "zoneName": "cool name for the zone based on the terrain type",
    "zoneDescription": "description of the corresponding zone"
  }
]`;
    terrainDetails = await doJSONCompletion(terrainPrompt, 1, 2048, 1);   

    // Generate terrain based on terrain type and description
    if (terrainType === "forest") {
      terrain = {
        description: terrainDetails,
        trees: Math.floor(Math.random() * 1000) + 1,
        bushes: Math.floor(Math.random() * 1500) + 1,
        rocks: Math.floor(Math.random() * 500) + 1
      };
    } else if (terrainType === "mountain") {
      terrain = {
        description: terrainDetails,
        trees: Math.floor(Math.random() * 200) + 1,
        bushes: Math.floor(Math.random() * 500) + 1,
        rocks: Math.floor(Math.random() * 1500) + 1
      };
    } else if (terrainType === "desert") {
      terrain = {
        description: terrainDetails,
        trees: Math.floor(Math.random() * 10),
        bushes: Math.floor(Math.random() * 50) + 1,
        cacti: Math.floor(Math.random() * 50) + 1,
        rocks: Math.floor(Math.random() * 1500) + 1
      };
    } else if (terrainType === "river") {
      terrain = {
        description: terrainDetails,
        fish: Math.floor(Math.random() * 200) + 1,
        rocks: Math.floor(Math.random() * 500) + 1,
        trees: Math.floor(Math.random() * 100)
      };
    } else if (terrainType === "town") {
      terrain = {
        description: terrainDetails,
        trees: Math.floor(Math.random() * 20) + 1,
        rocks: Math.floor(Math.random() * 50) + 1,
        buildings: Math.floor(Math.random() * 50) + 1,
        citizens: Math.floor(Math.random() * 50) + 1
      };  
      console.log(terrain);
    }

    for (let i = 0; i < terrainDetails.length; i++) {
       zoneData.zoneName = terrainDetails[i][0].zoneName;
       zoneData.zoneDescription = terrainDetails[i][0].zoneDescription;
    }

    return { 
      x: location.x, 
      y: location.y, 
      //terrainType: terrainType, 
      terrain: terrain 
    };

} catch (err) {
  console.error(err);
  throw err;
  } 
}

//generate noise from terrain details return noiseMap;
async function generateNoiseMap(location) {
  //const { terrain } = await generateLocationTerrain(location);
  //const { zoneDescription } = terrain.description[0];
  //console.log(zoneData.zoneDescription);
  const descriptionWords = zoneData.zoneDescription.split(' ');
  const noiseSeed = descriptionWords.reduce((acc, curr) => acc + curr.charCodeAt(0), 0);

  const noiseMap = new noise.Noise();
  noiseMap.seed(noiseSeed);

  return noiseMap;
}



// Generate NPCs based on terrain type using OpenAI GPT-3
async function generateNPCs(terrainType) {
  try {
    if (configuration.apiKey) {
      // Generate NPCs based on terrain type
      let npcs = [];
      let humanoids = [];
      let wildlife = [];
      let rareCreatures = [];
      let legendaryCreatures = [];
      let mythicalCreatures = [];

      //get humanoids 
      console.log('spawning humanoid creatures');
      let npcListPrompt = `Generate a list of humanoid NPCs that would inhabit ${terrainType} terrain with the following description: "${zoneData.zoneDescription}". The list should be realistic and enhance the game world. Exclusively use the following JSON response format and ensure appropriate markup: 
      [
        {
          "name": "NPC name",
          "count": "# (e.g. 3)",
          "type": "hostile or non-hostile",
          "rarity": "common, uncommon, rare, exotic",
          "exp": "number of experience points awarded upon slaying. Based upon the rarity and overall type of the NPC and typically be very low",
          "drop_table": [
            {
              "crafting materials": {
                "name": "item name",
                "quantity": "# (e.g. 3)"
              }
            },
            {
              "gear": {
                "name": "item name",
                "slot": "helmet, gloves, chest, legs, boots, cape",
                "armor": "between 1-50 based on rarity of item",
                "rarity": "equivalent to rarity of the NPC"
              }
            },
            {
              "weapons": {
                "name": "item name",
                "type": "range, melee, magic",
                "damage": "between 1-50 based on rarity of item",
                "rarity": "equivalent to rarity of the NPC"
              }
            }
          ]
        }
      ]`;
      humanoids = await doJSONCompletion(npcListPrompt, 1, 2048, 1);  
      npcs.push(humanoids);  

      // Generate additional wildlife using OpenAI GPT-3
      // can implement a ${} variable to store rarity and then use that in the prompt instead of typing each out
      console.log('spawning wildlife creatures');
      let wildlifePrompt = `Generate a list of wildlife NPCs that would inhabit ${terrainType} terrain with the following description "${zoneData.zoneDescription}". The list should be realistic and enhance the game world. Exclusively use the following JSON format: 
      [
        {
          "name": "NPC name",
          "count": "# (e.g. 3)",
          "type": "hostile or non-hostile",
          "rarity": "common, uncommon, rare, exotic",
          "exp": "number of experience points awarded upon slaying. Based upon the rarity and overall type of the NPC and typically very low",
          "drop_table": [
            {
              "crafting materials": {
                "name": "item name",
                "quantity": "# (e.g. 3)"
              }
            },
            {
              "gear": {
                "name": "item name",
                "slot": "helmet, gloves, chest, legs, boots, cape",
                "armor": "between 1-50 based on rarity of item",
                "rarity": "equivalent to rarity of the NPC"
              }
            },
            {
              "weapons": {
                "name": "item name",
                "type": "range, melee, magic",
                "damage": "between 1-50 based on rarity of item",
                "rarity": "equivalent to rarity of the NPC"
              }
            }
          ]
        }
      ]`;
      wildlife = await doJSONCompletion(wildlifePrompt, 1, 2048, 1);  
      npcs.push(wildlife);  

      // Generate additional legendary creatures using OpenAI GPT-3
      if (rollForLegendary()) {
        // Proceed with the event
        console.log('spawning legendary creatures');
        let legendaryPrompt = `Generate a list of legendary NPCs (wildlife, monsters, humanoids) that would inhabit ${terrainType} terrain with the following description: "${zoneData.zoneDescription}". Be creative with ideas for legendary creatures. The list should be realistic and enhance the game world. Exclusively use the following JSON format: 
      [
        {
          "name": "NPC name",
          "count": "# (e.g. 3)",
          "type": "hostile or non-hostile",
          "rarity": "legendary",
          "exp": "number of experience points awarded upon slaying. Based upon the rarity and overall type of the NPC and typically very low for normal creatures",
          "drop_table": [
            {
              "crafting materials": {
                "name": "item name",
                "quantity": "# (e.g. 3)"
              }
            },
            {
              "gear": {
                "name": "item name",
                "slot": "helmet, gloves, chest, legs, boots, cape",
                "armor": "between between 50-70",
                "rarity": "legendary"
              }
            },
            {
              "weapons": {
                "name": "item name",
                "type": "range, melee, magic",
                "damage": "between between 50-70",
                "rarity": "legendary"
              }
            }
          ]
        }
      ]`;
        legendaryCreatures = await doJSONCompletion(legendaryPrompt, 1, 2048, 1);  
        npcs.push(legendaryCreatures);  
      } else {
        // Do not proceed with the event
        console.log('not spawning legendary creatures');
     }

     
      // Generate additional mythical creatures using OpenAI GPT-3
      if (rollForMythical()) {
        // Proceed with the event
        console.log('spawning mythical creatures');
        let mythicalPrompt = `Generate a list of mythical NPCs (wildlife, monsters, humanoids) that would inhabit ${terrainType} terrain with the following description: "${zoneData.zoneDescription}". Be creative with ideas for mythical creatures. The list should be realistic and enhance the game world. Exclusively use the following JSON format: 
      [
        {
          "name": "NPC name",
          "count": "# (e.g. 3)",
          "type": "hostile or non-hostile",
          "rarity": "mythical",
          "exp": "number of experience points awarded upon slaying. Based upon the rarity and overall type of the NPC and typically very low for normal creatures",
          "drop_table": [
            {
              "crafting materials": {
                "name": "item name",
                "quantity": "# (e.g. 3)"
              }
            },
            {
              "gear": {
                "name": "item name",
                "slot": "helmet, gloves, chest, legs, boots, cape",
                "armor": "between between 70-100",
                "rarity": "mythical"
              }
            },
            {
              "weapons": {
                "name": "item name",
                "type": "range, melee, magic",
                "damage": "between between 70-100",
                "rarity": "mythical"
              }
            }
          ]
        }
      ]`;
        mythicalCreatures = await doJSONCompletion(mythicalPrompt, 1, 2048, 1);  
        npcs.push(mythicalCreatures);
      } else {
        // Do not proceed with the event
        console.log('not spawning mythical creatures');
      }
      
      // Combine all NPCs and creatures
      const allNPCs = [].concat(npcs, wildlife, rareCreatures);
      return npcs;
    }
  } catch (err) {
    console.error(err);
    throw err;
  }
}
  
async function generateQuests(terrainType) {
  // Generate quests based on terrain type
  try {
    const randomNumber = Math.floor(Math.random() * 6);
    const prompt = `Generate a list of ${randomNumber} quests for ${terrainType} terrain type with the following description: "${zoneData.zoneDescription}". Exclusively use the following JSON format for each quest: 
[
  {
    "questTitle": "quest name",
    "questObjective": "quest objectives",
    "questDescription": "quest description and lore",
    "questRewards": {
      "experience": "reasonable number of experience points based on the difficulty of the quest",
      "gold": "specific number of gold coins almost always less than 100"
    }
  }
]`;

    const completionData = await doJSONCompletion(prompt, 0.88, 2048, 1);  
    return completionData;
  } catch (err) {
    console.error(err);
    throw err;
  }
}

//general completion handler for soliciting JSON data 
async function doJSONCompletion(prompt, temp, tokens, number) {
  try {
    const data = [];
    if (configuration.apiKey) {
        const completions = await openai.createCompletion({
            model: "text-davinci-003",
            prompt: prompt,
            temperature: temp,
            max_tokens: tokens,
            n: number
            //stop: ['\n']
          }).then((response) => {
            let choices = response.data.choices;
            choices.forEach(choice => {
              let JSONString = choice.text.replace(/`/g, "");
              JSONString = JSONString.replace(/\n/g, "");
              JSONString = JSONString.replace(/\\/g, '');
              //console.log(JSONString);
              console.log(JSON.parse(JSONString));
              data.push(JSON.parse(JSONString));
          });
          }).catch((err) => {
            console.log(err);
          });
      }
      return data;
    } catch (err) {
      console.error(err);
      throw err;
    }
}; 

function rollForMythical() {
  return Math.random() <= 0.01;
}

function rollForLegendary() {
  return Math.random() <= 0.035;
}


module.exports = {
    generateTerrainContent,
    generateNPCs,
    generateQuests,
    generateLocationTerrain,
  };
