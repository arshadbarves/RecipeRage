const express = require('express');
const path = require('path');
const admin = require('firebase-admin');
require('dotenv').config();

const app = express();
const PORT = process.env.PORT || 3000;

// Initialize Firebase Admin SDK
let remoteConfig;
try {
    const serviceAccountPath = path.join(__dirname, 'service-account-key.json');
    const fs = require('fs');

    if (fs.existsSync(serviceAccountPath)) {
        const serviceAccount = require(serviceAccountPath);
        admin.initializeApp({
            credential: admin.credential.cert(serviceAccount),
        });
        console.log('✓ Firebase Admin SDK initialized successfully');
        remoteConfig = admin.remoteConfig();
    } else {
        console.warn('⚠ service-account-key.json not found.');
        console.warn('  Download from: Firebase Console > Project Settings > Service Accounts');
    }
} catch (error) {
    if (error.code === 'app/duplicate-app') {
        console.log('✓ Firebase app already initialized');
        remoteConfig = admin.remoteConfig();
    } else {
        console.error('✗ Error initializing Firebase Admin SDK:', error.message);
    }
}

// Helper function to get valid Firebase tag color
function getTagColor(environment) {
    // Firebase Remote Config valid colors: BLUE, BROWN, CYAN, DEEP_ORANGE, GREEN, 
    // INDIGO, LIME, ORANGE, PINK, PURPLE, TEAL
    if (environment === 'production') {
        return 'PINK';  // Use PINK for production (closest to red)
    } else if (environment === 'staging') {
        return 'ORANGE';
    } else {
        return 'BLUE';  // Development
    }
}

// Middleware
app.use(express.json());
app.use(express.static('public'));

// Basic auth middleware (optional)
const basicAuth = (req, res, next) => {
    const username = process.env.ADMIN_USERNAME;
    const password = process.env.ADMIN_PASSWORD;

    if (!username || !password) {
        return next(); // Skip auth if not configured
    }

    const auth = req.headers.authorization;
    if (!auth) {
        res.setHeader('WWW-Authenticate', 'Basic realm="Admin Dashboard"');
        return res.status(401).send('Authentication required');
    }

    const [user, pass] = Buffer.from(auth.split(' ')[1], 'base64').toString().split(':');
    if (user === username && pass === password) {
        next();
    } else {
        res.status(401).send('Invalid credentials');
    }
};

app.use(basicAuth);

// API Routes

// Get current config with platform/environment filtering
app.get('/api/config', async (req, res) => {
    const { platform = 'iOS', environment = 'production' } = req.query;

    if (!remoteConfig) {
        return res.status(503).json({
            error: 'Firebase not initialized',
            message: 'Please add service-account-key.json to enable Firebase integration'
        });
    }

    try {
        const template = await remoteConfig.getTemplate();

        // Extract parameters and apply conditional values based on platform/environment
        const parameters = {};
        const conditionName = `${platform.toLowerCase()}_${environment.toLowerCase()}`;

        for (const [key, param] of Object.entries(template.parameters)) {
            let value;
            let hasConditionalValue = false;

            try {
                // Check if there's a conditional value for this platform/environment
                if (param.conditionalValues && param.conditionalValues[conditionName]) {
                    value = JSON.parse(param.conditionalValues[conditionName].value);
                    hasConditionalValue = true;
                } else {
                    // Use default value
                    value = JSON.parse(param.defaultValue.value);
                }
            } catch (e) {
                // If parsing fails, use raw value
                value = param.conditionalValues?.[conditionName]?.value || param.defaultValue.value;
            }

            parameters[key] = {
                value,
                description: param.description || '',
                hasConditionalValue
            };
        }

        res.json({
            parameters,
            conditions: template.conditions || [],
            version: template.version?.versionNumber || 'unknown',
            platform,
            environment,
            conditionName
        });
    } catch (error) {
        console.error('Error fetching config:', error);
        res.status(500).json({
            error: 'Failed to fetch config',
            message: error.message
        });
    }
});

// Update single parameter with platform/environment support
app.put('/api/config/:key', async (req, res) => {
    const { key } = req.params;
    const { value, platform = 'iOS', environment = 'production', applyToAll = false } = req.body;

    if (!remoteConfig) {
        return res.status(503).json({
            error: 'Firebase not initialized'
        });
    }

    try {
        const template = await remoteConfig.getTemplate();
        const conditionName = `${platform.toLowerCase()}_${environment.toLowerCase()}`;

        // Create condition if it doesn't exist (using device.os which is valid)
        const conditionExists = template.conditions.some(c => c.name === conditionName);
        if (!conditionExists && !applyToAll) {
            template.conditions.push({
                name: conditionName,
                expression: `device.os == '${platform.toLowerCase()}'`,
                tagColor: getTagColor(environment)
            });
        }

        // Update or create parameter with conditional values
        if (!template.parameters[key]) {
            template.parameters[key] = {
                defaultValue: { value: JSON.stringify(value) },
                description: `Config for ${key}`,
                valueType: 'JSON'
            };
        } else {
            // Ensure existing parameter has JSON type
            template.parameters[key].valueType = 'JSON';
        }

        if (applyToAll) {
            // Update default value (applies to all)
            template.parameters[key].defaultValue = {
                value: JSON.stringify(value)
            };
        } else {
            // Set conditional value for this platform/environment
            if (!template.parameters[key].conditionalValues) {
                template.parameters[key].conditionalValues = {};
            }
            template.parameters[key].conditionalValues[conditionName] = {
                value: JSON.stringify(value)
            };
        }

        // Validate and publish
        const validatedTemplate = await remoteConfig.validateTemplate(template);
        const publishedTemplate = await remoteConfig.publishTemplate(validatedTemplate);

        res.json({
            success: true,
            version: publishedTemplate.version?.versionNumber || 'unknown',
            appliedTo: applyToAll ? 'all' : conditionName
        });
    } catch (error) {
        console.error('Error updating config:', error);
        res.status(500).json({
            error: 'Failed to update config',
            message: error.message
        });
    }
});

// Batch update
app.put('/api/config', async (req, res) => {
    const { updates, platform = 'iOS', environment = 'production', applyToAll = false } = req.body;

    if (!remoteConfig) {
        return res.status(503).json({
            error: 'Firebase not initialized'
        });
    }

    try {
        const template = await remoteConfig.getTemplate();
        const conditionName = `${platform.toLowerCase()}_${environment.toLowerCase()}`;

        // Create condition if needed
        const conditionExists = template.conditions.some(c => c.name === conditionName);
        if (!conditionExists && !applyToAll) {
            template.conditions.push({
                name: conditionName,
                expression: `device.os == '${platform.toLowerCase()}'`,
                tagColor: getTagColor(environment)
            });
        }

        // Update all parameters with conditional values
        for (const [key, value] of Object.entries(updates)) {
            if (!template.parameters[key]) {
                template.parameters[key] = {
                    defaultValue: { value: JSON.stringify(value) },
                    description: `Config for ${key}`,
                    valueType: 'JSON'
                };
            } else {
                // Ensure existing parameter has JSON type
                template.parameters[key].valueType = 'JSON';
            }

            if (applyToAll) {
                template.parameters[key].defaultValue = {
                    value: JSON.stringify(value)
                };
            } else {
                if (!template.parameters[key].conditionalValues) {
                    template.parameters[key].conditionalValues = {};
                }
                template.parameters[key].conditionalValues[conditionName] = {
                    value: JSON.stringify(value)
                };
            }
        }

        // Validate and publish
        const validatedTemplate = await remoteConfig.validateTemplate(template);
        const publishedTemplate = await remoteConfig.publishTemplate(validatedTemplate);

        res.json({
            success: true,
            version: publishedTemplate.version?.versionNumber || 'unknown',
            updated: Object.keys(updates).length,
            appliedTo: applyToAll ? 'all' : conditionName
        });
    } catch (error) {
        console.error('Error updating config:', error);
        res.status(500).json({
            error: 'Failed to update config',
            message: error.message
        });
    }
});

// Get all conditions
app.get('/api/conditions', async (req, res) => {
    if (!remoteConfig) {
        return res.status(503).json({
            error: 'Firebase not initialized'
        });
    }

    try {
        const template = await remoteConfig.getTemplate();
        res.json({
            conditions: template.conditions || []
        });
    } catch (error) {
        console.error('Error fetching conditions:', error);
        res.status(500).json({
            error: 'Failed to fetch conditions',
            message: error.message
        });
    }
});

// Health check
app.get('/api/health', async (req, res) => {
    res.json({
        status: 'ok',
        firebaseInitialized: !!remoteConfig,
        authMethod: 'firebase_admin_sdk'
    });
});

// Serve dashboard
app.get('*', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

app.listen(PORT, () => {
    console.log(`\n🚀 Firebase Config Dashboard running at http://localhost:${PORT}`);

    if (remoteConfig) {
        console.log(`✓ Firebase Admin SDK connected`);
    } else {
        console.log(`⚠ Add service-account-key.json for Firebase integration`);
        console.log(`  Download from: Firebase Console > Project Settings > Service Accounts`);
    }

    if (process.env.ADMIN_USERNAME && process.env.ADMIN_PASSWORD) {
        console.log(`🔒 Basic Auth enabled (user: ${process.env.ADMIN_USERNAME})`);
    } else {
        console.log(`⚠️  Warning: No dashboard authentication configured`);
    }

    console.log();
});
