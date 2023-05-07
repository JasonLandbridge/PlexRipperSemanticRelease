import { take } from 'rxjs/operators';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import LibraryService from '@service/libraryService';
import ServerService from '@service/serverService';
import { generatePlexLibraries, generatePlexServers, generateResultDTO } from '@mock';
import { PLEX_LIBRARY_RELATIVE_PATH, PLEX_SERVER_RELATIVE_PATH } from '@api-urls';

describe('LibraryService.getServerByLibraryId()', () => {
	let { mock, config } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
	});

	test('Should return the correct server when given a valid libraryId', async () => {
		// Arrange
		config = {
			seed: 23695,
			plexServerCount: 3,
			plexLibraryCount: 20,
		};

		const servers = generatePlexServers({ config });
		const libraries = servers.map((x) => generatePlexLibraries({ plexServerId: x.id, config })).flat();

		mock.onGet(PLEX_SERVER_RELATIVE_PATH).reply(200, generateResultDTO(servers));
		mock.onGet(PLEX_LIBRARY_RELATIVE_PATH).reply(200, generateResultDTO(libraries));
		const serverSetup$ = ServerService.setup();
		const librarySetup$ = LibraryService.setup();
		const testLibrary = libraries[5];
		// Act
		const serverSetupResult = subscribeSpyTo(serverSetup$);
		const librarySetupResult = subscribeSpyTo(librarySetup$);
		await serverSetupResult.onComplete();
		await librarySetupResult.onComplete();
		const serverByLibraryId$ = LibraryService.getServerByLibraryId(testLibrary.id).pipe(take(1));
		const serverByLibraryIdResult = subscribeSpyTo(serverByLibraryId$);
		await serverByLibraryIdResult.onComplete();

		// Assert
		expect(serverSetupResult.receivedComplete()).toEqual(true);
		expect(librarySetupResult.receivedComplete()).toEqual(true);
		const values = serverByLibraryIdResult.getValues();
		expect(values).toHaveLength(1);
		expect(serverByLibraryIdResult.getFirstValue()?.id).toEqual(testLibrary.plexServerId);
	});
});
