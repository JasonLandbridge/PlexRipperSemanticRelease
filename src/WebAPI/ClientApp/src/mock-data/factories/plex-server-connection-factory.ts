import { randIp, randPort } from '@ngneat/falso';
import { times } from 'lodash-es';
import { checkConfig, incrementSeed, type MockConfig } from '~/mock-data';
import type { PlexServerConnectionDTO, PlexServerDTO, PlexServerStatusDTO, ServerConnectionCheckStatusProgressDTO } from '@dto';

let plexServerConnectionIdIndex = 1;

export function generatePlexServerConnection({
	id,
	plexServerId,
	partialData = {},
}: {
	id: number;
	plexServerId: number;
	partialData?: Partial<PlexServerConnectionDTO>;
}): PlexServerConnectionDTO {
	incrementSeed(id);
	const scheme = 'http';
	const host = randIp();
	const port = randPort();

	return {
		id,
		protocol: scheme,
		address: host,
		port,
		url: `${scheme}://${host}:${port}`,
		iPv4: true,
		iPv6: false,
		local: false,
		relay: false,
		portFix: false,
		progress: {} as ServerConnectionCheckStatusProgressDTO,
		serverStatusList: [],
		latestConnectionStatus: {} as PlexServerStatusDTO,
		plexServerId,
		...partialData,
	};
}

export function generatePlexServerConnections({
	plexServerId,
	config = {},
	partialData = {},
}: {
	plexServerId: number;
	config: Partial<MockConfig>;
	partialData?: Partial<PlexServerDTO>;
}): PlexServerConnectionDTO[] {
	const validConfig = checkConfig(config);
	return times(validConfig.maxServerConnections, () =>
		generatePlexServerConnection({ id: plexServerConnectionIdIndex++, plexServerId, partialData }),
	);
}
