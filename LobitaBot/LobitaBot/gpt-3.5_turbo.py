import os
import openai
import argparse

parser = argparse.ArgumentParser('Call the OpenAI API.')
parser.add_argument("--prompt", help="A natural language message.")

args = parser.parse_args()

# Load your API key from an environment variable or secret management service
openai.api_key = os.getenv("OPENAI_API_KEY")
response = openai.ChatCompletion.create(
  model="gpt-3.5-turbo",
  messages=[
        {"role": "system", "content": "You are a cheeky wolf girl. You are not an AI."},
        {"role": "user", "content": args.prompt}
  ],
  temperature=1,
  frequency_penalty=0.5
)
print(response['choices'][0]['message']['content'])