import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '~~/tests/_base/base';
import {
	DOWNLOAD_RELATIVE_PATH,
	FOLDER_PATH_RELATIVE_PATH,
	NOTIFICATION_RELATIVE_PATH,
	PLEX_ACCOUNT_RELATIVE_PATH,
	PLEX_LIBRARY_RELATIVE_PATH,
	PLEX_SERVER_CONNECTION_RELATIVE_PATH,
	PLEX_SERVER_RELATIVE_PATH,
	SETTINGS_RELATIVE_PATH,
} from '@api-urls';
import { generatePlexServers, generateResultDTO, generateSettingsModel } from '@mock';

describe('GlobalStore.getConfigReady()', () => {
	let { appConfig, mock, config } = baseVars();
	beforeAll(() => {
		const result = baseSetup();
		appConfig = result.appConfig;
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		config = {
			plexServerCount: 3,
		};
		const globalStore = useGlobalStore();

		mock.onGet(DOWNLOAD_RELATIVE_PATH).reply(200, generateResultDTO([]));
		mock.onGet(PLEX_ACCOUNT_RELATIVE_PATH).reply(200, generateResultDTO([]));
		mock.onGet(FOLDER_PATH_RELATIVE_PATH).reply(200, generateResultDTO([]));
		mock.onGet(PLEX_LIBRARY_RELATIVE_PATH).reply(200, generateResultDTO([]));
		mock.onGet(NOTIFICATION_RELATIVE_PATH).reply(200, generateResultDTO([]));
		mock.onGet(PLEX_SERVER_RELATIVE_PATH).reply(200, generateResultDTO(generatePlexServers({ config })));
		mock.onGet(SETTINGS_RELATIVE_PATH).reply(200, generateResultDTO(generateSettingsModel({ config })));
		mock.onGet(PLEX_SERVER_CONNECTION_RELATIVE_PATH).reply(200, generateResultDTO([]));

		// Act
		const setupResult = subscribeSpyTo(globalStore.setupServices({ config: appConfig }));
		const pageSetupResult = subscribeSpyTo(globalStore.getPageSetupReady);
		await setupResult.onComplete();
		// Assert
		expect(setupResult.receivedComplete()).toEqual(true);
		expect(pageSetupResult.getValues()).toHaveLength(1);
		expect(pageSetupResult.getFirstValue()).toEqual(true);
	});
});
