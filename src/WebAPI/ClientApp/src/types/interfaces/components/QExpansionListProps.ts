export interface QExpansionListProps {
	icon?: string;
	title?: string;

	noTranslate?: boolean;
	link?: string;
	type?: 'badge' | 'server' | 'library';
	count?: number;
	children?: QExpansionListProps[];
}
