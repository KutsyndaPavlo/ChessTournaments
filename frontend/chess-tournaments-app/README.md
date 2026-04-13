# Chess Tournaments App

A modern Angular application for managing chess tournaments with OIDC authentication.

## Quick Start

### 1. Install Dependencies

```bash
npm install
```

### 2. Configure Environment (First Time Only)

Create your local environment configuration:

```bash
# Copy the example environment file
cp src/environments/environment.example.ts src/environments/environment.local.ts

# Edit src/environments/environment.local.ts with your OIDC settings
```

See [ENVIRONMENT.md](ENVIRONMENT.md) for detailed configuration guide.

### 3. Start Development Server

```bash
# Default development environment
npm start

# Or use your local configuration
npm run start:local
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`.

## Environment Configuration

The application supports multiple environments for OIDC configuration:

```bash
# Development (default)
npm start

# Local (custom settings - recommended for development)
npm run start:local

# Staging
npm run start:staging

# Production
npm run start:prod
```

**📖 Read [ENVIRONMENT.md](ENVIRONMENT.md) for complete environment configuration guide.**

## Authentication

This app uses **Authorization Code Flow with PKCE** for secure authentication.

**📖 Read [AUTHENTICATION.md](AUTHENTICATION.md) for authentication setup guide.**

### Quick OIDC Setup

1. Ensure your identity server is running on `https://localhost:7225/`
2. Register the SPA client in your identity server (see [AUTHENTICATION.md](AUTHENTICATION.md))
3. Configure OIDC settings in `src/environments/environment.local.ts`
4. Run `npm run start:local`

## Development server

To start a local development server, run:

```bash
npm start
# or
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Vitest](https://vitest.dev/) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
