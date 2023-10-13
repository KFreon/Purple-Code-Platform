<script lang="ts">
	import type { Data } from '$lib/data';
	import Card from './card.svelte';

	let cards: Data[] = [
		{
			id: 'best',
			upvotes: 56,
			code: `console.log('lol, this is passed in');`,
			comments: [],
			created: '2023-10-12',
			modified: '2023-10-13',
			snippetTitle: 'The best code eva'
		}
	];

	const emptyCardData: Data = {
		id: new Date().toISOString(),
		upvotes: 0,
		code: '',
		comments: [],
		created: new Date().toDateString(),
		modified: new Date().toDateString(),
		snippetTitle: ''
	};

	function addNewSnippet() {
		cards = [emptyCardData, ...cards];
	}

	function deleteSnippet(data: Data) {
		cards = cards.filter((x) => x.snippetTitle !== data.snippetTitle);
	}
</script>

<div class="main-container">
	<div class="header">
		<h1>Purple Code Platform</h1>
		<p>We write so much code, but sometimes we lose track of the good bits.</p>
		<p>Here we can save the bits that others (or just us) might want to use again later.</p>
	</div>

	<button class="new-button" on:click={addNewSnippet}>Add new</button>
	<ul class="code-area">
		{#each cards as card (card.id)} <!-- keyed so we can prepend -->
			<Card data={card} onDelete={(item) => deleteSnippet(item)} />
		{/each}
	</ul>
</div>

<style src="./page.css"></style>
