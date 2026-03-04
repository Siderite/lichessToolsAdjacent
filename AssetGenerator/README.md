# LiChess Tools Asset generator
Generates various files and other data used by [LiChess Tools](https://github.com/Siderite/lichessTools/)

Instructions:
1. change whatever you need in the project's **Data** folder and build the project
OR
1. change whatever you need in the **bin/Data** folder, just remember that a build will overwrite the files there
2. make sure the Crowdin API token is stored in the environment variable **CROWDIN_TOKEN**
3. run the AssetGenerator.exe program
4. copy:
  - countries.json
  - crowdin.json
  - flairs.json
  - gambits.json
  - openings.json
  - wikiUrls.json
  to the LiChess Tools **data** folder
5. copy lichess-icons.js to the LiChess Tools **scripts** folder
6. copy lichess-icons.css to the LiChess Tools **styles** folder
