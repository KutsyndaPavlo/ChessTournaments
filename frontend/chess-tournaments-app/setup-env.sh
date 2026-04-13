#!/bin/bash

# Chess Tournaments - Environment Setup Script
# This script helps you set up your local environment configuration

echo "♔ Chess Tournaments - Environment Setup ♔"
echo ""

ENV_DIR="src/environments"
EXAMPLE_FILE="$ENV_DIR/environment.example.ts"
LOCAL_FILE="$ENV_DIR/environment.local.ts"

# Check if example file exists
if [ ! -f "$EXAMPLE_FILE" ]; then
    echo "❌ Error: $EXAMPLE_FILE not found!"
    exit 1
fi

# Check if local file already exists
if [ -f "$LOCAL_FILE" ]; then
    echo "⚠️  Warning: $LOCAL_FILE already exists!"
    read -p "Do you want to overwrite it? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "Setup cancelled."
        exit 0
    fi
fi

# Copy example to local
echo "📄 Creating $LOCAL_FILE from example..."
cp "$EXAMPLE_FILE" "$LOCAL_FILE"

echo ""
echo "✅ Environment file created successfully!"
echo ""
echo "📝 Next steps:"
echo "1. Edit $LOCAL_FILE"
echo "2. Configure your OIDC settings (issuer, clientId, etc.)"
echo "3. Run: npm run start:local"
echo ""
echo "📖 For detailed configuration guide, see ENVIRONMENT.md"
echo ""
