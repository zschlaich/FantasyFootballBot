# FantasyFootballBot
Project for my fantasy football league. Integrates Azure OpenAI models with a Discord bot to generate text based off prompts sent by league members. Members of the Discord will be able to prompt the bot to generate power rankings for each of the teams, provide matchup information, generate graphs, and more.

## Where do your stats come from?
There is a phenomenal GitHub repository named [NFL-Data](https://github.com/hvpkod/NFL-Data/tree/main) maintained by user hvpkod. Please visit that repository for more information on where the data comes from and how it is collected. This project utilizes the JSON files uploaded for each position group every week for all of its stat calculations.

## Future plans?
I would love to explore the potential of making customizable versions of this bot available to other fantasy groups, but that's likely a ways away at this point. Still working on the basic functionality for this one ;)

## TODO
- add logic to generate power rankings
- add graphing functionality
  - add the ability for the bot to send graphs as images in the Discord server
- find a way to hide tenant ID and GitHub PAT in code