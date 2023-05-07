# MiroslavGPT Telegram Bot

This repository contains a custom Telegram bot that interacts with OpenAI's ChatGPT API. The bot can be used in personal conversations and group chats to generate human-like responses based on user input. The project consists of two main components: `MiroslavGPT.Domain`, which contains the reusable business logic for the bot, and `MiroslavGPT.Azure`, which is a wrapper for Azure Functions and Azure Cosmos DB deployment.

The bot is created with the help of ChatGPT (GPT-4) from OpenAI.

## Features

- Authorization using a secret key
- Supports commands in direct messages and group chats
- Supports multiple personalities
- Supports threading with replies
- Processes prompts and sends responses using OpenAI's ChatGPT API

## Components

- `MiroslavGPT.Domain`: Contains the reusable business logic for the Telegram bot, making it easy to integrate with other projects or platforms
- `MiroslavGPT.Azure`: Provides an Azure Functions and Azure Cosmos DB prepared wrapper for deploying the bot on AWS infrastructure

## Commands
### Init
- `/init {secret_key}` -- Authorizes a user to use the bot by verifying the provided secret key
### Personality prompts
Sends a message to the bot with pregenerated personality messages
- `/prompt {prompt}` -- Default personality
- `/dan {prompt}` -- DAN jailbreak personality
- `/tsundere {prompt}` -- a tsundere girl personality
- `/bravo {prompt}` -- Johnny Bravo personality
- `/inverse {prompt}` -- Inverse (good-to-bad) personality

## Dependencies

- OpenAI API (OpenAI by OkGoDoIt): Interacts with the ChatGPT API
- Azure Cosmos DB: Stores authorized user data and threads
- Azure Functions: Hosts the webhook function

## Contributing
This project is for personal use only. Contributions are not accepted at this time.

## License
This project is licensed under the MIT License. See [LICENSE](LICENSE.md) for more details.
