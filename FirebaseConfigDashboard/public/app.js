let currentConfig = {};

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    loadConfig();
    setupTabs();
    setupRefreshButton();
});

// Tab switching
function setupTabs() {
    const tabBtns = document.querySelectorAll('.tab-btn');
    const tabContents = document.querySelectorAll('.tab-content');

    tabBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            const tabName = btn.dataset.tab;

            tabBtns.forEach(b => b.classList.remove('active'));
            tabContents.forEach(c => c.classList.remove('active'));

            btn.classList.add('active');
            document.getElementById(`${tabName}-tab`).classList.add('active');
        });
    });
}

// Refresh button
function setupRefreshButton() {
    document.getElementById('refresh-btn').addEventListener('click', loadConfig);

    // Reload config when platform or environment changes
    const platformSelect = document.getElementById('platform-select');
    const environmentSelect = document.getElementById('environment-select');

    if (platformSelect) {
        platformSelect.addEventListener('change', loadConfig);
    }
    if (environmentSelect) {
        environmentSelect.addEventListener('change', loadConfig);
    }
}

// Load config from server
async function loadConfig() {
    try {
        showNotification('Loading configuration...', 'info');

        const platform = document.getElementById('platform-select')?.value || 'iOS';
        const environment = document.getElementById('environment-select')?.value || 'production';

        const response = await fetch(`/api/config?platform=${platform}&environment=${environment}`);
        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.error || 'Failed to load config');
        }

        currentConfig = data.parameters;

        // Update version info
        const versionInfo = `Version: ${data.version} | ${data.platform} - ${data.environment}`;
        document.getElementById('version-info').textContent = versionInfo;

        // Populate all editors
        populateEditor('map-editor', 'MapConfig');
        populateEditor('shop-editor', 'ShopConfig');
        populateEditor('character-editor', 'CharacterConfig');
        populateEditor('skins-editor', 'SkinsConfig');
        populateEditor('settings-editor', 'GameSettings');
        populateEditor('maintenance-editor', 'MaintenanceConfig');
        populateEditor('update-editor', 'ForceUpdateConfig');

        showNotification(`✅ Configuration loaded (${data.platform} - ${data.environment})`, 'success');
    } catch (error) {
        console.error('Error loading config:', error);
        showNotification(`❌ Error: ${error.message}`, 'error');
    }
}

// Populate JSON editor
function populateEditor(editorId, configKey) {
    const editor = document.getElementById(editorId);
    const config = currentConfig[configKey];

    if (config) {
        editor.value = JSON.stringify(config.value, null, 2);
    } else {
        editor.value = '{\n  \n}';
    }
}

// These functions are no longer needed - using JSON editors instead

// Update config
async function updateConfig(configKey, editorId) {
    try {
        const editor = document.getElementById(editorId);
        const value = JSON.parse(editor.value);

        const platform = document.getElementById('platform-select')?.value || 'iOS';
        const environment = document.getElementById('environment-select')?.value || 'production';

        showNotification('Saving...', 'info');

        const response = await fetch(`/api/config/${configKey}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                value,
                platform,
                environment
            })
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.error || 'Failed to update config');
        }

        showNotification(`✅ ${configKey} updated for ${data.appliedTo} (v${data.version})`, 'success');
        await loadConfig();
    } catch (error) {
        console.error('Error updating config:', error);
        showNotification(`❌ Error: ${error.message}`, 'error');
    }
}



// Show notification
function showNotification(message, type) {
    const notification = document.getElementById('notification');
    notification.textContent = message;
    notification.className = `notification ${type} show`;

    setTimeout(() => {
        notification.classList.remove('show');
    }, 3000);
}

// Helper functions removed - no longer needed with JSON editors
