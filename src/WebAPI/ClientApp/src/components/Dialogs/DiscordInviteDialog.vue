<template>
	<QCardDialog
		:name="DialogType.DiscordServerInviteDialog"
		width="600px"
		close-button
		persistent
		button-align="center"
		@closed="onClosed">
		<template #title>
			{{ $t('components.discord-invite-dialog.header') }}
		</template>
		<template #default>
			<ul class="q-mt-none">
				<li>{{ $t('components.discord-invite-dialog.reasons.reason-1') }}</li>
				<li>{{ $t('components.discord-invite-dialog.reasons.reason-2') }}</li>
				<li>{{ $t('components.discord-invite-dialog.reasons.reason-3') }}</li>
				<li>{{ $t('components.discord-invite-dialog.reasons.reason-4') }}</li>
				<li>{{ $t('components.discord-invite-dialog.reasons.reason-5') }}</li>
			</ul>
		</template>
		<template #actions="{ close }">
			<BaseButton
				block
				@click="onInviteClick(close)">
				<DiscordIcon class="q-mr-md" />
				<QText :value="$t('components.discord-invite-dialog.discord-invite-button-text')" />
			</BaseButton>
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import { DialogType } from '@enums';

const settingsStore = useSettingsStore();

function onInviteClick(close: () => void): void {
	const discordLink = 'https://discord.com/invite/Qa3BtxN77g';
	window.open(discordLink, '_blank');
	settingsStore.generalSettings.hasBeenInvitedToDiscord = true;
	close();
}

function onClosed(): void {
	settingsStore.generalSettings.hasBeenInvitedToDiscord = true;
}
</script>
