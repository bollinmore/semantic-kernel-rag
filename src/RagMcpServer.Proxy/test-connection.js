const axios = require("axios");

const API_BASE_URL = "http://localhost:5228";

async function testQuery() {
  try {
    console.log("Testing connection to C# Server...");
    
    // Test 1: Using PascalCase 'Query' (as in current index.js)
    console.log("\nTest 1: Sending { Query: 'What is this project?' }");
    try {
      const response1 = await axios.post(`${API_BASE_URL}/Query`, {
        Query: "What is this project?",
      });
      console.log("Response 1 Status:", response1.status);
      console.log("Response 1 Data:", JSON.stringify(response1.data, null, 2));
    } catch (e) {
      console.error("Test 1 Failed:", e.message, e.response?.data);
    }

    // Test 2: Using camelCase 'query' (Standard JSON)
    console.log("\nTest 2: Sending { query: 'What is this project?' }");
    try {
      const response2 = await axios.post(`${API_BASE_URL}/Query`, {
        query: "What is this project?",
      });
      console.log("Response 2 Status:", response2.status);
      console.log("Response 2 Data:", JSON.stringify(response2.data, null, 2));
    } catch (e) {
      console.error("Test 2 Failed:", e.message, e.response?.data);
    }

  } catch (error) {
    console.error("Fatal Error:", error);
  }
}

testQuery();
