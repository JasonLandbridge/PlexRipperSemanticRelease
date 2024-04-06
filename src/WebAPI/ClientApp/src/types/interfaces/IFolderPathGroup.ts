import { type FolderPathDTO, FolderType, PlexMediaType } from '@dto';

export default interface IFolderPathGroup {
	header: string;
	paths: FolderPathDTO[];
	mediaType: PlexMediaType;
	folderType: FolderType;
	isFolderNameEditable: boolean;
	isFolderAddable: boolean;
	IsFolderDeletable: boolean;
}
