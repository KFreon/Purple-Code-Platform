<script lang="ts">
	import type { Data } from '$lib/data';
	import { onMount } from 'svelte';
	import Card from '$lib/components/card/card.svelte';

	let cards: Data[] = [];

  let promise: Promise<unknown>;

  onMount(() => {
    // Get data
    promise = fetch('https://localhost:5001/snippets')
      .then(resp => resp.json())
      .then(x => {
        console.log('data', x);
        cards = x;
        return cards;
      });
  });

	const getEmptyCardData = ():Data => ({
		id: new Date().toISOString(),
		languageId: 'plaintext',
		upvotes: 0,
		code: '',
		comments: [],
		createdOn: new Date().toLocaleDateString('en-au', {year: 'numeric', month: '2-digit', day: '2-digit'}),
		modifiedOn: new Date().toLocaleDateString('en-au', {year: 'numeric', month: '2-digit', day: '2-digit'}),
		title: ''
	});

	function addNewSnippet() {
		cards = [getEmptyCardData(), ...cards];
	}

	function deleteSnippet(data: Data) {
		cards = cards.filter((x) => x.title !== data.title);

		promise = fetch(`https://localhost:5001/snippets/${data.languageId}/{${data.id}}`, {
      method: 'DELETE',
      headers: {
        "content-type": 'application/json'
      }
    })
	}

  function saveSnippet(snippet: Data) {
    snippet.id = snippet.title;
		snippet.modifiedOn = new Date().toLocaleDateString('en-au', {year: 'numeric', month: '2-digit', day: '2-digit'});
    promise = fetch('https://localhost:5001/snippets', {
      method: 'POST',
      body: JSON.stringify(snippet),
      headers: {
        "content-type": 'application/json'
      }
    })
  }
</script>

<div class="main-container">
	<div class="header">
		<h1>Purple Code Platform</h1>
		<p>We write so much code, but sometimes we lose track of the good bits.</p>
		<p>Here we can save the bits that others (or just us) might want to use again later.</p>
	</div>

	{#await promise}
  <div class='loading'>
    <p>🍵</p>
  </div>
  {:then resp} 
  <button class="new-button" on:click={addNewSnippet}>Add new</button>
	<div class='code-scroller'>
		<ul class="code-area">
			{#each cards as card (card.id)} <!-- keyed so we can prepend -->
				<li>
					<Card data={card} onDelete={(item) => deleteSnippet(item)} onSave={(item) => saveSnippet(item)} />
				</li>
			{/each}
		</ul>
	</div>
  {/await}
</div>

<style src="./page.css"></style>
