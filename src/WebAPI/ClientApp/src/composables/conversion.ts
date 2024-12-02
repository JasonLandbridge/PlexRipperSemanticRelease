import type { DownloadMediaDTO, PlexMediaDTO, PlexMediaSlimDTO } from '@dto';
import { DownloadStatus } from '@dto';
import { useI18n } from 'vue-i18n';

export function toDownloadMedia(mediaItem: PlexMediaDTO | PlexMediaSlimDTO): DownloadMediaDTO[] {
	return [
		{
			mediaIds: [mediaItem.id],
			type: mediaItem.type,
			plexServerId: mediaItem.plexServerId,
			plexLibraryId: mediaItem.plexLibraryId,
		},
	];
}

export function toFullThumbUrl({
	connectionUrl,
	mediaKey,
	MetaDataKey,
	token,
	width,
	height,
}: {
	connectionUrl: string;
	mediaKey: number;
	MetaDataKey: number;
	token: string;
	width?: number;
	height?: number;
}): string {
	return `${connectionUrl}/photo/:/transcode?url=/library/metadata/${mediaKey}/thumb/${MetaDataKey}&minSize=1&upscale=1&width=${width}&height=${height}&X-Plex-Token=${token}`;
}

export function translateDownloadStatus(status: DownloadStatus) {
	const { t } = useI18n();

	switch (status) {
		case DownloadStatus.Unknown:
			return t('general.download-status.unknown');
		case DownloadStatus.DownloadFinished:
			return t('general.download-status.download-finished');
		case DownloadStatus.Deleted:
			return t('general.download-status.deleted');
		case DownloadStatus.Merging:
			return t('general.download-status.merging');
		case DownloadStatus.Moving:
			return t('general.download-status.moving');
		case DownloadStatus.Completed:
			return t('general.download-status.completed');
		case DownloadStatus.Downloading:
			return t('general.download-status.downloading');
		case DownloadStatus.Error:
			return t('general.download-status.error');
		case DownloadStatus.ServerUnreachable:
			return t('general.download-status.server-unreachable');
		case DownloadStatus.Paused:
			return t('general.download-status.paused');
		case DownloadStatus.Queued:
			return t('general.download-status.queued');
		case DownloadStatus.Stopped:
			return t('general.download-status.stopped');
		case DownloadStatus.MergePaused:
			return t('general.download-status.merge-paused');
		case DownloadStatus.MovePaused:
			return t('general.download-status.move-paused');
		case DownloadStatus.MergeFinished:
			return t('general.download-status.merge-finished');
		case DownloadStatus.MoveFinished:
			return t('general.download-status.move-finished');
		case DownloadStatus.MoveError:
			return t('general.download-status.move-error');
		case DownloadStatus.MergeError:
			return t('general.download-status.merge-error');
		default:
			return t('general.error.unknown');
	}
}
