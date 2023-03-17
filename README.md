# MiroslavGPT Telegram Bot

This repository contains a custom Telegram bot that interacts with OpenAI's ChatGPT API. The bot can be used in personal conversations and group chats to generate human-like responses based on user input. The project consists of two main components: `MiroslavGPT.Domain`, which contains the reusable business logic for the bot, and `MiroslavGPT.AWS`, which is a wrapper for AWS Lambda and AWS DynamoDB deployment.

The bot is primarily created by ChatGPT (GPT-4) from OpenAI.

## Features

- Authorization using a secret key
- Supports commands in direct messages and group chats
- Processes prompts and sends responses using OpenAI's ChatGPT API

## Components

- `MiroslavGPT.Domain`: Contains the reusable business logic for the Telegram bot, making it easy to integrate with other projects or platforms
- `MiroslavGPT.AWS`: Provides an AWS Lambda and AWS DynamoDB prepared wrapper for deploying the bot on AWS infrastructure

## Commands

- `/init {secret_key}` - Authorizes a user to use the bot by verifying the provided secret key
- `/prompt {prompt}` - Sends the prompt to ChatGPT API and returns its answer in reply to the command message

## Setup

1. Clone the repository:

```bash
git clone https://github.com/yourusername/miroslav-gpt-bot.git
cd miroslav-gpt-bot
```

2. Set up the required environment variables:

- `CHATGPT_SECRET_KEY`: The secret key for authorizing users
- `OPENAI_API_KEY`: Your OpenAI API key
- `TELEGRAM_BOT_TOKEN`: Your Telegram bot token
- `TELEGRAM_BOT_USERNAME`: Your Telegram bot username
- `DYNAMODB_TABLE_NAME`: The name of the DynamoDB table for storing authorized users
- `MAX_TOKENS`: The maximum number of tokens for the ChatGPT API response

3. Deploy the Telegram bot to AWS Lambda:

- Create an AWS Lambda function using the .NET runtime and upload the compiled project
- Set up the Lambda function's environment variables with the values specified in step 2
- Increase the Lambda function's timeout and memory settings as needed for the ChatGPT API interactions

4. Configure the webhook URL to receive updates from the Telegram API:

- Create an Amazon API Gateway to trigger the Lambda function
- Set up an HTTPS endpoint with a POST method and integrate it with the Lambda function
- Use the `/setWebhook` method of the Telegram Bot API to set the webhook URL to the API Gateway endpoint

5. Set up the DynamoDB table with the following schema:

- Primary key: `chatId` (Number)

6. Authorize users by sending the `/init {secret_key}` command in a private chat or group chat with the bot.

7. Start using the `/prompt {prompt}` command to interact with the ChatGPT API.

## Dependencies

- OpenAI API (OpenAI by OkGoDoIt): Interacts with the ChatGPT API
- Amazon DynamoDB: Stores authorized user data
- Amazon Lambda: Hosts the webhook function

## Contributing

This project is for personal use only. Contributions are not accepted at this time.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE.md) for more details.
